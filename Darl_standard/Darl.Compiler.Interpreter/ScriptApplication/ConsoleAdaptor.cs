// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ConsoleAdaptor.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace DarlCompiler.Interpreter
{
    /// <summary>
    /// Enum ConsoleTextStyle
    /// </summary>
    public enum ConsoleTextStyle
    {
        /// <summary>
        /// The normal
        /// </summary>
        Normal,
        /// <summary>
        /// The error
        /// </summary>
        Error,
    }

    // Default implementation of IConsoleAdaptor with System Console as input/output. 
    /// <summary>
    /// Class ConsoleAdapter.
    /// </summary>
    public class ConsoleAdapter : IConsoleAdaptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAdapter"/> class.
        /// </summary>
        public ConsoleAdapter()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        /// <summary>
        /// Handles the CancelKeyPress event of the Console control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConsoleCancelEventArgs"/> instance containing the event data.</param>
        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; //do not kill the app yet
            Canceled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IConsoleAdaptor" /> is canceled.
        /// </summary>
        /// <value><c>true</c> if canceled; otherwise, <c>false</c>.</value>
        public bool Canceled { get; set; }

        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Write(string text)
        {
            Console.Write(text);
        }
        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="text">The text.</param>
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
        /// <summary>
        /// Sets the text style.
        /// </summary>
        /// <param name="style">The style.</param>
        public void SetTextStyle(ConsoleTextStyle style)
        {
            switch (style)
            {
                case ConsoleTextStyle.Normal:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case ConsoleTextStyle.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
        }

        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int Read()
        {
            return Console.Read();
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ReadLine()
        {
            var input = Console.ReadLine();
            Canceled = (input == null);  // Windows console method ReadLine returns null if Ctrl-C was pressed.
            return input;
        }
        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetTitle(string title)
        {
            Console.Title = title;
        }
    }


}
