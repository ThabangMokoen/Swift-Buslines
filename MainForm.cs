using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace SwiftBus;

public class MainForm : Form
{
    private readonly Color Navy = Color.FromArgb(18, 28, 45);
    private readonly Color Blue = Color.FromArgb(42, 120, 220);
    private readonly Color Light = Color.FromArgb(245, 247, 250);
    private readonly Color Green = Color.FromArgb(34, 160, 107);
    private readonly Color Red = Color.FromArgb(220, 70, 70);

    private TextBox txtName = null!, txtPhone = null!;
    private ComboBox cmbFrom = null!, cmbTo = null!, cmbType = null!, cmbPayment = null!;
    private DateTimePicker dtDate = null!, dtDeparture = null!;
    private NumericUpDown numPassengers = null!;
    private DataGridView passengerGrid = null!;
    private FlowLayoutPanel seatPanel = null!;
    private RichTextBox ticketPreview = null!;
    private Label lblFare = null!, lblDiscount = null!, lblSchedule = null!, lblSelectedSeat = null!, lblStatus = null!, lblPaymentStatus = null!;
    private Button btnBook = null!, btnPay = null!, btnPrint = null!;
    private int selectedSeat = -1;
    private bool paymentComplete;
    private string currentBookingNumber = "";
    private decimal currentTotal;
    private decimal currentDiscount;
    private readonly HashSet<int> occupiedSeats = new() { 3, 7, 12, 18 };
    private readonly decimal[] fares = { 250m, 350m, 450m };

    private readonly Dictionary<string, int> routeDurations = new()
    {
        ["Johannesburg|Pretoria"] = 60, ["Pretoria|Johannesburg"] = 60,
        ["Johannesburg|Durban"] = 360, ["Durban|Johannesburg"] = 360,
        ["Johannesburg|Bloemfontein"] = 300, ["Bloemfontein|Johannesburg"] = 300,
        ["Johannesburg|Cape Town"] = 900, ["Cape Town|Johannesburg"] = 900,
        ["Pretoria|Durban"] = 420, ["Durban|Pretoria"] = 420,
        ["Pretoria|Cape Town"] = 960, ["Cape Town|Pretoria"] = 960,
        ["Durban|Cape Town"] = 1260, ["Cape Town|Durban"] = 1260,
        ["Bloemfontein|Cape Town"] = 720, ["Cape Town|Bloemfontein"] = 720,
        ["Pretoria|Bloemfontein"] = 240, ["Bloemfontein|Pretoria"] = 240,
        ["Durban|Bloemfontein"] = 390, ["Bloemfontein|Durban"] = 390
    };

    public MainForm()
    {
        Text = "SwiftBus - Bus Ticket Booking System";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 850);
        MinimumSize = new Size(1100, 750);
        BackColor = Light;
        Font = new Font("Segoe UI", 10);

        BuildUi();
        ResetForm();
    }

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Navy };
        var title = new Label { Text = "SWIFTBUS", ForeColor = Color.White, Font = new Font("Segoe UI", 25, FontStyle.Bold), AutoSize = true, Location = new Point(28, 15) };
        var sub = new Label { Text = "Simple • Safe • Comfortable travel", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(31, 57) };
        header.Controls.Add(title); header.Controls.Add(sub);
        Controls.Add(header);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 1 };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        Controls.Add(main);

        var left = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var right = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        main.Controls.Add(left, 0, 0); main.Controls.Add(right, 1, 0);

        var booking = Card("BOOKING DETAILS");
        booking.Location = new Point(0, 0); booking.Size = new Size(650, 570);
        left.Controls.Add(booking);

        AddLabel(booking, "Passenger contact", 22, 52);
        txtName = InputBox(booking, 22, 76, 290, "Full name");
        txtPhone = InputBox(booking, 330, 76, 290, "Phone number");

        AddLabel(booking, "Route", 22, 125);
        cmbFrom = Combo(booking, 22, 149, 290, new[] { "Johannesburg", "Pretoria", "Durban", "Bloemfontein", "Cape Town" });
        cmbTo = Combo(booking, 330, 149, 290, new[] { "Johannesburg", "Pretoria", "Durban", "Bloemfontein", "Cape Town" });

        AddLabel(booking, "Travel date", 22, 198);
        dtDate = new DateTimePicker { Location = new Point(22, 222), Size = new Size(180, 32), Format = DateTimePickerFormat.Short, MinDate = DateTime.Today };
        booking.Controls.Add(dtDate);

        AddLabel(booking, "Departure time", 225, 198);
        dtDeparture = new DateTimePicker { Location = new Point(225, 222), Size = new Size(120, 32), Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true };
        booking.Controls.Add(dtDeparture);

        AddLabel(booking, "Bus type", 365, 198);
        cmbType = Combo(booking, 365, 222, 130, new[] { "Standard", "Comfort", "Executive" });
        AddLabel(booking, "Passengers", 515, 198);
        numPassengers = new NumericUpDown { Location = new Point(515, 222), Size = new Size(105, 32), Minimum = 1, Maximum = 5, Value = 1 };
        booking.Controls.Add(numPassengers);

        AddLabel(booking, "Passengers & age-based discounts", 22, 270);
        passengerGrid = new DataGridView
        {
            Location = new Point(22, 296), Size = new Size(598, 150),
            AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
        passengerGrid.Columns.Add("Passenger", "Passenger");
        passengerGrid.Columns.Add("Age", "Age");
        passengerGrid.Columns.Add("Category", "Category");
        passengerGrid.Columns.Add("Discount", "Discount");
        booking.Controls.Add(passengerGrid);

        lblFare = AddLabel(booking, "Total: R0.00", 22, 460, true);
        lblDiscount = AddLabel(booking, "Discount: R0.00", 22, 488);
        lblSchedule = AddLabel(booking, "Schedule: select a route", 22, 516);

        var seatsCard = Card("SEAT SELECTION");
        seatsCard.Location = new Point(0, 590); seatsCard.Size = new Size(650, 245);
        left.Controls.Add(seatsCard);
        lblSelectedSeat = AddLabel(seatsCard, "Selected seat: None", 22, 50);
        seatPanel = new FlowLayoutPanel { Location = new Point(22, 82), Size = new Size(598, 105), WrapContents = true };
        seatsCard.Controls.Add(seatPanel);
        var legend = new Label { Text = "GREEN = Available     RED = Occupied     BLUE = Selected", AutoSize = true, Location = new Point(22, 195), ForeColor = Color.DimGray };
        seatsCard.Controls.Add(legend);

        var payment = Card("PAYMENT & CONFIRMATION");
        payment.Location = new Point(0, 850); payment.Size = new Size(650, 185);
        left.Controls.Add(payment);
        AddLabel(payment, "Payment method", 22, 50);
        cmbPayment = Combo(payment, 22, 74, 230, new[] { "Card", "EFT / Bank Transfer", "Cash" });
        btnPay = ActionButton("SIMULATE PAYMENT", Blue, 270, 72, 190, payment);
        lblPaymentStatus = AddLabel(payment, "Payment: NOT PAID", 22, 120);
        btnBook = ActionButton("BOOK TICKET", Green, 465, 72, 155, payment);
        btnBook.Click += BookTicket;
        btnPay.Click += SimulatePayment;

        var previewCard = Card("TICKET PREVIEW");
        previewCard.Location = new Point(0, 0); previewCard.Size = new Size(530, 650);
        right.Controls.Add(previewCard);
        ticketPreview = new RichTextBox { Location = new Point(18, 55), Size = new Size(494, 510), ReadOnly = true, Font = new Font("Consolas", 10), BackColor = Color.White };
        previewCard.Controls.Add(ticketPreview);
        btnPrint = ActionButton("PRINT TICKET", Navy, 18, 580, 160, previewCard);
        btnPrint.Click += PrintTicket;
        var clear = ActionButton("CLEAR / RESET", Color.Gray, 190, 580, 160, previewCard);
        clear.Click += (_, _) => ResetForm();
        lblStatus = AddLabel(previewCard, "Ready for booking", 18, 620);
    }

    private Panel Card(string title)
    {
        var p = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Navy, AutoSize = true, Location = new Point(20, 18) });
        return p;
    }

    private Label AddLabel(Control parent, string text, int x, int y, bool bold = false)
    {
        var l = new Label { Text = text, AutoSize = true, Location = new Point(x, y), ForeColor = Navy, Font = new Font("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular) };
        parent.Controls.Add(l); return l;
    }

    private TextBox InputBox(Control parent, int x, int y, int w, string placeholder)
    {
        var t = new TextBox { Location = new Point(x, y), Size = new Size(w, 32), PlaceholderText = placeholder };
        parent.Controls.Add(t); return t;
    }

    private ComboBox Combo(Control parent, int x, int y, int w, string[] items)
    {
        var c = new ComboBox { Location = new Point(x, y), Size = new Size(w, 32), DropDownStyle = ComboBoxStyle.DropDownList };
        c.Items.AddRange(items); c.SelectedIndex = 0; c.SelectedIndexChanged += (_, _) => UpdateCalculations();
        parent.Controls.Add(c); return c;
    }

    private Button ActionButton(string text, Color color, int x, int y, int w, Control parent)
    {
        var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 40), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        b.FlatAppearance.BorderSize = 0; parent.Controls.Add(b); return b;
    }

    private void ResetForm()
    {
        txtName.Clear(); txtPhone.Clear();
        cmbFrom.SelectedIndex = 0; cmbTo.SelectedIndex = 1; cmbType.SelectedIndex = 0; cmbPayment.SelectedIndex = 0;
        dtDate.Value = DateTime.Today; dtDeparture.Value = DateTime.Today.AddHours(8);
        numPassengers.Value = 1;
        selectedSeat = -1; paymentComplete = false; currentBookingNumber = "";
        passengerGrid.Rows.Clear();
        passengerGrid.Rows.Add("Passenger 1", 30, "Adult", "0%");
        ticketPreview.Text = InitialTicketText();
        lblPaymentStatus.Text = "Payment: NOT PAID"; lblStatus.Text = "Ready for booking";
        btnPay.Text = "SIMULATE PAYMENT"; btnBook.Text = "BOOK TICKET"; btnPrint.Enabled = false;
        RefreshSeatButtons(); UpdateCalculations();
    }

    private void RefreshPassengerRows()
    {
        int count = (int)numPassengers.Value;
        while (passengerGrid.Rows.Count < count) passengerGrid.Rows.Add($"Passenger {passengerGrid.Rows.Count + 1}", 30, "Adult", "0%");
        while (passengerGrid.Rows.Count > count) passengerGrid.Rows.RemoveAt(passengerGrid.Rows.Count - 1);
        for (int i = 0; i < count; i++) UpdatePassengerRow(i);
    }

    private void UpdatePassengerRow(int row)
    {
        int age = 30;
        if (passengerGrid.Rows[row].Cells["Age"].Value != null)
            int.TryParse(passengerGrid.Rows[row].Cells["Age"].Value.ToString(), out age);
        age = Math.Clamp(age, 0, 120);
        var rate = GetDiscountRate(age);
        passengerGrid.Rows[row].Cells["Age"].Value = age;
        passengerGrid.Rows[row].Cells["Category"].Value = GetPassengerCategory(age);
        passengerGrid.Rows[row].Cells["Discount"].Value = $"{rate:P0}";
    }

    private decimal GetDiscountRate(int age)
    {
        if (age <= 5) return 1m;
        if (age <= 17) return 0.25m;
        if (age >= 60) return 0.30m;
        return 0m;
    }

    private string GetPassengerCategory(int age)
    {
        if (age <= 5) return "Infant";
        if (age <= 17) return "Child";
        if (age >= 60) return "Senior";
        return "Adult";
    }

    private decimal CalculateTotal(out decimal discount)
    {
        decimal baseFare = fares[Math.Max(0, cmbType.SelectedIndex)];
        discount = 0m; decimal total = 0m;
        foreach (DataGridViewRow row in passengerGrid.Rows)
        {
            int age = Convert.ToInt32(row.Cells["Age"].Value);
            decimal d = baseFare * GetDiscountRate(age);
            discount += d;
            total += baseFare - d;
        }
        return total;
    }

    private void UpdateCalculations()
    {
        if (passengerGrid == null) return;
        RefreshPassengerRows();
        currentTotal = CalculateTotal(out currentDiscount);
        lblFare.Text = $"Total: R{currentTotal:N2}";
        lblDiscount.Text = $"Discount: R{currentDiscount:N2}";
        UpdateSchedule();
    }

    private void UpdateSchedule()
    {
        if (cmbFrom.SelectedItem?.ToString() == cmbTo.SelectedItem?.ToString())
        {
            lblSchedule.Text = "Schedule: FROM and TO cannot be the same";
            return;
        }
        string key = $"{cmbFrom.SelectedItem}|{cmbTo.SelectedItem}";
        int minutes = routeDurations.TryGetValue(key, out var m) ? m : 480;
        DateTime departure = dtDate.Value.Date.Add(dtDeparture.Value.TimeOfDay);
        DateTime eta = departure.AddMinutes(minutes);
        lblSchedule.Text = $"Departure: {departure:HH:mm}    ETA: {eta:dd MMM HH:mm}    Duration: {minutes / 60}h {minutes % 60}m";
    }

    private void RefreshSeatButtons()
    {
        seatPanel.Controls.Clear();
        for (int i = 1; i <= 20; i++)
        {
            int seatNumber = i;
            bool occupied = occupiedSeats.Contains(i);
            var b = new Button
            {
                Text = occupied ? $"{i} X" : i.ToString(),
                Size = new Size(75, 38),
                BackColor = occupied ? Red : (selectedSeat == i ? Blue : Color.White),
                ForeColor = occupied || selectedSeat == i ? Color.White : Navy,
                FlatStyle = FlatStyle.Flat,
                Enabled = !occupied
            };
            b.Click += (_, _) => SelectSeat(seatNumber);
            seatPanel.Controls.Add(b);
        }
        lblSelectedSeat.Text = selectedSeat > 0 ? $"Selected seat: {selectedSeat}" : "Selected seat: None";
    }

    private void SelectSeat(int seat)
    {
        if (occupiedSeats.Contains(seat))
        {
            MessageBox.Show("That seat is already occupied.", "Seat unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        selectedSeat = seat; RefreshSeatButtons();
    }

    private void SimulatePayment(object? sender, EventArgs e)
    {
        if (selectedSeat < 0) { ShowWarning("Select a seat before payment."); return; }
        currentTotal = CalculateTotal(out currentDiscount);
        var result = MessageBox.Show(
            $"Passenger count: {passengerGrid.Rows.Count}\nPayment method: {cmbPayment.Text}\nAmount: R{currentTotal:N2}\n\nThis is a simulated payment. No real money will be charged.\n\nApprove payment?",
            "SwiftBus Payment Simulation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result == DialogResult.Yes)
        {
            paymentComplete = true;
            lblPaymentStatus.Text = "Payment: PAID ✓";
            btnPay.Text = "PAYMENT SUCCESS ✓";
        }
    }

    private void BookTicket(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text)) { ShowWarning("Enter the passenger booking contact name."); return; }
        if (string.IsNullOrWhiteSpace(txtPhone.Text)) { ShowWarning("Enter a phone number."); return; }
        if (cmbFrom.Text == cmbTo.Text) { ShowWarning("Choose different departure and destination locations."); return; }
        if (selectedSeat < 0) { ShowWarning("Select an available seat."); return; }
        if (occupiedSeats.Contains(selectedSeat)) { ShowWarning("That seat has become occupied. Select another seat."); return; }
        if (!paymentComplete) { ShowWarning("Complete the simulated payment first."); return; }

        currentBookingNumber = "SB" + DateTime.Now.ToString("yyMMddHHmmss");
        currentTotal = CalculateTotal(out currentDiscount);
        string key = $"{cmbFrom.Text}|{cmbTo.Text}";
        int minutes = routeDurations.TryGetValue(key, out var m) ? m : 480;
        DateTime departure = dtDate.Value.Date.Add(dtDeparture.Value.TimeOfDay);
        DateTime eta = departure.AddMinutes(minutes);

        occupiedSeats.Add(selectedSeat);
        ticketPreview.Text = BuildTicket(departure, eta, minutes);
        lblStatus.Text = $"Booking {currentBookingNumber} CONFIRMED";
        btnBook.Text = "BOOKED ✓";
        btnPrint.Enabled = true;
        RefreshSeatButtons();
    }

    private string BuildTicket(DateTime departure, DateTime eta, int minutes)
    {
        var lines = new List<string>
        {
            "==========================================",
            "              SWIFTBUS TICKET",
            "==========================================",
            $"Booking No. : {currentBookingNumber}",
            $"Contact     : {txtName.Text}",
            $"Phone       : {txtPhone.Text}",
            $"FROM        : {cmbFrom.Text}",
            $"TO          : {cmbTo.Text}",
            $"DATE        : {dtDate.Value:dd MMM yyyy}",
            $"DEPARTURE   : {departure:HH:mm}",
            $"ETA         : {eta:dd MMM HH:mm}",
            $"DURATION    : {minutes / 60}h {minutes % 60}m",
            $"BUS TYPE    : {cmbType.Text}",
            $"SEAT        : {selectedSeat}",
            $"PASSENGERS  : {passengerGrid.Rows.Count}",
            "------------------------------------------",
            "PASSENGER FARES"
        };
        foreach (DataGridViewRow row in passengerGrid.Rows)
        {
            int age = Convert.ToInt32(row.Cells["Age"].Value);
            string category = GetPassengerCategory(age);
            decimal rate = GetDiscountRate(age);
            decimal baseFare = fares[cmbType.SelectedIndex];
            decimal final = baseFare * (1 - rate);
            lines.Add($"{row.Index + 1}. Age {age} | {category,-7} | {rate:P0} | R{final:N2}");
        }
        lines.Add("------------------------------------------");
        lines.Add($"BASE FARE   : R{fares[cmbType.SelectedIndex] * passengerGrid.Rows.Count:N2}");
        lines.Add($"DISCOUNT    : R{currentDiscount:N2}");
        lines.Add($"TOTAL       : R{currentTotal:N2}");
        lines.Add($"PAYMENT     : {cmbPayment.Text}");
        lines.Add("STATUS      : CONFIRMED / PAID");
        lines.Add("==========================================");
        lines.Add("Thank you for travelling with SwiftBus.");
        return string.Join(Environment.NewLine, lines);
    }

    private void PrintTicket(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(currentBookingNumber)) { ShowWarning("Book a ticket before printing."); return; }
        using var document = new PrintDocument();
        document.PrintPage += (_, args) =>
        {
            using var font = new Font("Consolas", 9);
            float y = 60;
            args.Graphics.DrawString("SWIFTBUS", new Font("Segoe UI", 20, FontStyle.Bold), Brushes.Black, 60, 20);
            foreach (string line in ticketPreview.Text.Split(Environment.NewLine))
            {
                args.Graphics.DrawString(line, font, Brushes.Black, 60, y);
                y += 16;
                if (y > args.PageBounds.Height - 30) break;
            }
        };
        using var dialog = new PrintDialog { Document = document, UseEXDialog = true };
        if (dialog.ShowDialog() == DialogResult.OK) document.Print();
    }

    private string InitialTicketText() =>
@"==========================================
              SWIFTBUS TICKET
==========================================

Book a ticket to generate the full ticket.

Features:
• Individual passenger ages
• Automatic age discounts
• Seat assignment
• Departure and ETA
• Simulated payment
• Printable ticket

==========================================";

    private void ShowWarning(string message) =>
        MessageBox.Show(message, "SwiftBus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
