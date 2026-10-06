using ArpPoison_ng.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArpPoison_ng.Services
{
    public class ConsoleUI : IConsoleUI
    {
        public void ShowBanner(string text)
        {
            AnsiConsole.Write(
                new FigletText(text)
                .Color(Color.Green)
            );
        }

        public void Write(string message)
        {
            AnsiConsole.MarkupLine($"[bold white]{Markup.Escape(message)}[/]");
        }

        public void WriteLine(string message)
        {
            AnsiConsole.MarkupLine($"[bold white]{Markup.Escape(message)}[/]");
        }
    }
}
