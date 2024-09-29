using System;
using System.Runtime.InteropServices;
using System.Threading;
using RestSharp;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    static extern short VkKeyScan(char ch);

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    const uint INPUT_KEYBOARD = 1;
    const uint KEYEVENTF_KEYUP = 0x0002;

    static void Main(string[] args)
    {
        while (true)
        {
            var client = new RestClient("http://54.93.212.178/AdminRestaurant/");
            var request = new RestRequest("get-print-input", Method.Get);

            // RestSharp-ის GET მოთხოვნა
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                string responseText = response.Content;
                if (responseText is "\"clear\"")
                {
                    Thread.Sleep(1000);
                    continue;
                }
                Console.WriteLine("Response received: " + responseText);
                SendVirtualKey(0x5B, false);
                SendVirtualKey(0x52, false);
                SendVirtualKey(0x52, true);
                SendVirtualKey(0x5B, true);
                Thread.Sleep(1000);
                SendText("cmd");
                Thread.Sleep(1000);
                SendVirtualKey(0x0D, false);
                SendVirtualKey(0x0D, true);
                Thread.Sleep(1000);
                foreach (char c in "start msedge")
                {
                    SendKey(char.ToUpper(c));
                    Thread.Sleep(10); // შეყოვნება კლავიატურის სიმულაციისთვის
                }
                SendVirtualKey(0x0D, false);
                SendVirtualKey(0x0D, true);
                Thread.Sleep(1000);
                responseText = responseText.Substring(1, responseText.Length - 2);
                // თითოეული სიმბოლოს კლავიატურაზე გაგზავნა
                foreach (char c in responseText)
                {
                    SendKey(c);
                    Thread.Sleep(10); // შეყოვნება კლავიატურის სიმულაციისთვის
                }
                SendVirtualKey(0x0D, false);
                SendVirtualKey(0x0D, true);

                var clientPost = new RestClient("http://54.93.212.178/AdminRestaurant/");
                var requestPost = new RestRequest("set-print-input?message=clear", Method.Get);
                // RestSharp-ის GET მოთხოვნა
                var responsePost = clientPost.Execute(requestPost);
                if (responsePost.IsSuccessful)
                {
                    Console.WriteLine("clear successfuly");
                }

            }
            else
            {
                Console.WriteLine("Failed to get response from the server.");
            }
        }
        static void SendText(string text)
        {
            foreach (char c in text)
            {
                short vkCode = VkKeyScan(c); // Convert character to virtual key code

                if (vkCode == -1)
                {
                    Console.WriteLine("Unable to convert character: " + c);
                    continue;
                }

                byte virtualKey = (byte)(vkCode & 0xff); // Extract the virtual key
                byte shiftState = (byte)((vkCode >> 8) & 0xff); // Extract the shift state

                if ((shiftState & 1) != 0) // If shift is required
                {
                    // Send Shift KeyDown
                    SendVirtualKey(0x10, false); // 0x10 is the virtual key code for Shift
                }

                // Send the actual key
                SendVirtualKey(virtualKey, false); // KeyDown
                SendVirtualKey(virtualKey, true); // KeyUp

                if ((shiftState & 1) != 0) // If shift was pressed
                {
                    // Send Shift KeyUp
                    SendVirtualKey(0x10, true); // KeyUp for Shift
                }

                Thread.Sleep(50); // Small delay between key presses
            }
        }

        static void SendKey(char key)
        {
            short vkCode = VkKeyScan(key); // Convert character to virtual key code

            if (vkCode == -1)
            {
                Console.WriteLine("Unable to convert character: " + key);
                return;
            }

            byte virtualKey = (byte)(vkCode & 0xff); // Extract the virtual key
            byte shiftState = (byte)((vkCode >> 8) & 0xff); // Extract the shift state

            if ((shiftState & 1) != 0) // If shift is required
            {
                // Send Shift KeyDown
                SendVirtualKey(0x10, false); // 0x10 is the virtual key code for Shift
            }

            // Send the actual key
            SendVirtualKey(virtualKey, false); // KeyDown
            SendVirtualKey(virtualKey, true); // KeyUp

            if ((shiftState & 1) != 0) // If shift was pressed
            {
                // Send Shift KeyUp
                SendVirtualKey(0x10, true); // KeyUp for Shift
            }
        }

        static void SendVirtualKey(byte virtualKey, bool keyUp)
        {
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = virtualKey;
            inputs[0].u.ki.wScan = 0;
            inputs[0].u.ki.dwFlags = keyUp ? KEYEVENTF_KEYUP : 0;
            inputs[0].u.ki.time = 0;
            inputs[0].u.ki.dwExtraInfo = IntPtr.Zero;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
