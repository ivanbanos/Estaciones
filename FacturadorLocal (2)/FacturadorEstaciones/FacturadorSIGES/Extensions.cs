using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacturadorSIGES
{
    public static class Extensions
    {
        public static async Task CloseAfterDelay(this Form form, int millisecondsDelay)
        {
            await Task.Delay(millisecondsDelay);
            form.Close();
        }
    }
}
