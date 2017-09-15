using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScpControl.Shared.Core;
using Bindables;

namespace ScpControlPanel.Controls
{
    public delegate void PadPromotedEventHandler(object sender, DsPadId padId);

    /// <summary>
    ///     Interaction logic for PadEntryControl.xaml
    /// </summary>
    public partial class PadEntryControl : UserControl
    {
        #region Ctor

        public PadEntryControl()
        {
            InitializeComponent();

            PadId = DsPadId.None;
            PadType = DsModel.DS3;
            MacAddress = "00:00:00:00:00:00";
            ConnectionType = DsConnection.None;
            PacketCounter = 0;
            BatteryStatus = DsBattery.None;
        }

        #endregion

        #region Dependency properties

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public bool IsTopPad { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsPadId PadId { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsModel PadType { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public string MacAddress { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsConnection ConnectionType { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public int PacketCounter { get; set; }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public DsBattery BatteryStatus { get; set; }

        #endregion

        #region MVVM commands

        private ICommand _promoteCommand;

        public ICommand PromoteCommand
        {
            get
            {
                return _promoteCommand ?? (_promoteCommand = new RelayCommand(
                    param => PromotePad(),
                    param => CanPromote()
                    ));
            }
        }

        private bool CanPromote()
        {
            // Verify command can be executed here

            return (PadId != DsPadId.One);
        }

        private void PromotePad()
        {
            // Save command execution logic
            OnPadPromoted(PadId);
        }

        #endregion

        #region Events

        public event PadPromotedEventHandler Promoted;

        private void OnPadPromoted(DsPadId padId)
        {
            if (Promoted != null)
                Promoted(this, padId);
        }

        #endregion
    }

    /// <summary>
    ///     A command whose sole purpose is to
    ///     relay its functionality to other
    ///     objects by invoking delegates. The
    ///     default return value for the CanExecute
    ///     method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        ///     Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null || _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

        #endregion // ICommand Members
    }
}