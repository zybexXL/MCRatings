using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

public static class ExtensionMethods
{
    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    {
        Type dgvType = dgv.GetType();
        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        pi.SetValue(dgv, setting, null);
    }

    public static DateTime EpochToDateTime(long epoch)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
    }

    // JRiver seems to have an off-by-1 bug in the dates it uses in this old Lotus123 format
    // epoch is 30.12.1989 instead of 31.12.1899
    public static int DaysSince1900(DateTime date)
    {
        return (int)(date - new DateTime(1899, 12, 30)).TotalDays;
    }

    // word-wraps string into lines of max width
    public static string WordWrap(this string text, int width)
    {
        int pos, next;
        StringBuilder sb = new StringBuilder();

        // Lucidity check
        if (width < 1 || text.Length <= width)
            return text;

        text = text.Replace("\r\n", "\n");
        // Parse each line of text
        for (pos = 0; pos < text.Length; pos = next)
        {
            // Find end of line
            int eol = text.IndexOf("\n", pos);

            if (eol == -1)
                next = eol = text.Length;
            else
                next = eol + "\n".Length;

            // Copy this line of text, breaking into smaller lines as needed
            if (eol > pos)
            {
                do
                {
                    int len = eol - pos;

                    if (len > width)
                        len = BreakLine(text, pos, width);

                    sb.Append(text, pos, len);
                    sb.Append("\n");

                    // Trim whitespace following break
                    pos += len;

                    while (pos < eol && Char.IsWhiteSpace(text[pos]))
                        pos++;

                } while (eol > pos);
            }
            else sb.Append("\n"); // Empty line
        }

        return sb.ToString();
    }

    /// <summary>
    /// Locates position to break the given line so as to avoid
    /// breaking words.
    /// </summary>
    /// <param name="text">String that contains line of text</param>
    /// <param name="pos">Index where line of text starts</param>
    /// <param name="max">Maximum line length</param>
    /// <returns>The modified line length</returns>
    private static int BreakLine(string text, int pos, int max)
    {
        // Find last whitespace in line
        int i = max - 1;
        while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
            i--;
        if (i < 0)
            return max; // No whitespace found; break at maximum length
                        // Find start of whitespace
        while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
            i--;
        // Return length of text before whitespace
        return i + 1;
    }
}
