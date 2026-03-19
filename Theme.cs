namespace YTSDownloader;

internal static class Theme
{
    // Background colors
    public static readonly Color Background = Color.FromArgb(30, 30, 46);
    public static readonly Color Surface = Color.FromArgb(40, 42, 58);
    public static readonly Color SurfaceLight = Color.FromArgb(50, 52, 68);
    public static readonly Color SurfaceBorder = Color.FromArgb(65, 67, 83);

    // Text colors
    public static readonly Color TextPrimary = Color.FromArgb(205, 214, 244);
    public static readonly Color TextSecondary = Color.FromArgb(147, 153, 178);
    public static readonly Color TextMuted = Color.FromArgb(108, 112, 134);

    // Accent colors
    public static readonly Color Accent = Color.FromArgb(137, 180, 250);
    public static readonly Color AccentHover = Color.FromArgb(116, 160, 235);
    public static readonly Color Success = Color.FromArgb(166, 227, 161);
    public static readonly Color Warning = Color.FromArgb(249, 226, 175);
    public static readonly Color Error = Color.FromArgb(243, 139, 168);
    public static readonly Color Download = Color.FromArgb(180, 190, 254);

    // Input colors
    public static readonly Color InputBackground = Color.FromArgb(24, 24, 37);
    public static readonly Color InputBorder = Color.FromArgb(69, 71, 90);

    // Fonts
    public static readonly Font FontRegular = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font FontSmall = new("Segoe UI", 8.5f, FontStyle.Regular);
    public static readonly Font FontHeading = new("Segoe UI Semibold", 10f, FontStyle.Regular);
    public static readonly Font FontMono = new("Cascadia Mono", 9f, FontStyle.Regular);

    public static void ApplyTo(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = TextPrimary;
        form.Font = FontRegular;
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = InputBackground;
        textBox.ForeColor = TextPrimary;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = FontRegular;
    }

    public static void StyleButton(Button button, bool isPrimary = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.Font = FontRegular;
        button.Cursor = Cursors.Hand;

        if (isPrimary)
        {
            button.BackColor = Accent;
            button.ForeColor = Color.FromArgb(30, 30, 46);
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.MouseOverBackColor = AccentHover;
        }
        else
        {
            button.BackColor = Surface;
            button.ForeColor = TextPrimary;
            button.FlatAppearance.BorderColor = SurfaceBorder;
            button.FlatAppearance.MouseOverBackColor = SurfaceLight;
        }
    }

    public static void StyleGroupBox(GroupBox groupBox)
    {
        groupBox.ForeColor = TextSecondary;
        groupBox.Font = FontSmall;
        groupBox.BackColor = Color.Transparent;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = InputBackground;
        comboBox.ForeColor = TextPrimary;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = FontRegular;
    }

    public static void StyleCheckBox(CheckBox checkBox)
    {
        checkBox.ForeColor = TextPrimary;
        checkBox.Font = FontRegular;
        checkBox.BackColor = Color.Transparent;
    }

    public static void StyleLabel(Label label, bool isHeading = false)
    {
        label.ForeColor = isHeading ? TextPrimary : TextSecondary;
        label.Font = isHeading ? FontHeading : FontRegular;
        label.BackColor = Color.Transparent;
    }

    public static void StyleRichTextBox(RichTextBox rtb, bool isLog = false)
    {
        rtb.BackColor = isLog ? Color.FromArgb(17, 17, 27) : Surface;
        rtb.ForeColor = isLog ? TextSecondary : TextPrimary;
        rtb.Font = isLog ? FontMono : FontRegular;
        rtb.BorderStyle = BorderStyle.None;
    }

    public static void StylePanel(Panel panel, bool isSurface = false)
    {
        panel.BackColor = isSurface ? Surface : Background;
    }
}
