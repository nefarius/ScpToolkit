using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using ScpControl.ScpCore;

namespace ScpControl.Profiler
{
    public class VirtualMouse : SingletonBase<VirtualMouse>
    {
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly IMouseSimulator _mouseSimulator;

        private VirtualMouse()
        {
            _mouseSimulator = _inputSimulator.Mouse;
        }
    }
}
