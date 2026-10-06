using System;
using System.Collections.Generic;
using System.Text;

namespace ArpPoison_ng.Interfaces
{
    public interface IConsoleUI
    {
        void ShowBanner(string text);
        void Write(string message);
        void WriteLine(string message);
    }
}
