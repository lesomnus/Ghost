using AutoHotkey.Interop;
using System;
using System.Diagnostics;
using System.Text;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace Ghost
{
    public class Controller
    {
        [Flags]
        private enum _Action
        {
            None = 0,
            Hold = 1,
            Release = 1 << 1,
            WithCtrl = 1 << 2,
            WithAlt = 1 << 3,
            WithShift = 1 << 4,
        }

        private static AutoHotkeyEngine _engine = AutoHotkeyEngine.Instance;
        private string _trg;
        private _Action _action;

        static Controller()
        {
            _engine.ExecRaw("SendMode Input");
        }
        public Controller(Process process)
        {
            _trg = process.Id.ToString();
        }

        public Controller Send(in string key)
        {
            if (_action == _Action.None)
            {
                _engine.ExecRaw($"ControlSend, ahk_parent, {key}, ahk_pid, {_trg}");
                return this;
            }

            var sb = new StringBuilder();
            if ((_action & _Action.WithAlt) > 0) sb.Append("!");
            if ((_action & _Action.WithCtrl) > 0) sb.Append("^");
            if ((_action & _Action.WithShift) > 0) sb.Append("+");

            bool isSymbol = key[0] == '{';

            if ((_action & _Action.Hold) > 0
            || (_action & _Action.Release) > 0)
            {
                _action = _Action.None;
                return Send(sb.ToString());
            }

            if (isSymbol)
            {
                sb.Append(key);
                sb.Length--;
                sb.Append(' ');
            }
            else
            {
                sb.Append('{');
                sb.Append(key);
                sb.Append(' ');
            }

            if ((_action & _Action.Hold) > 0)
            {
                sb.Append("down");
            }
            else if ((_action & _Action.Release) > 0)
            {
                sb.Append("up");
            }

            _action = _Action.None;
            return Send(sb.ToString());
        }

        public Controller Click(in Rect rect, int times = 1)
        {
            return Click(rect.Location, rect.Size, times);
        }
        public Controller Click(in Point point, in Size size, int times = 1)
        {
            return Click(
                point.X + (size.Width / 2),
                point.Y + (size.Height / 2), times);
        }
        public Controller Click(in Point point, int times = 1)
        {
            return Click(point.X, point.Y, times);
        }
        public Controller Click(int x, int y, int times = 1)
        {
            _engine.ExecRaw($"Controlclick, x{x} y{y}, ahk_pid {_trg},,, {times},NA");
            return this;
        }

        public Controller Tab() { return Send("{Tab}"); }
        public Controller Enter() { return Send("{Enter}"); }
        public Controller Esc() { return Send("{Esc}"); }
        public Controller Up() { return Send("{Up}"); }
        public Controller Down() { return Send("{Down}"); }
        public Controller Left() { return Send("{Left}"); }
        public Controller Right() { return Send("{Right}"); }
        public Controller Home() { return Send("{Home}"); }
        public Controller Char(char key)
        {
            switch (key)
            {
                case '{':
                case '}':
                case '!':
                case '^':
                case '+':
                case '#':
                    return Send("{" + key + "}");
                default: break;
            }
            return Send(key.ToString());
        }
        public Controller Ctrl()
        {
            if((_action & _Action.Hold) != 0)
            {
                return Send("{Ctrl}");
            }

            _action |= _Action.WithCtrl;
            return this;
        }
        public Controller Alt()
        {
            if ((_action & _Action.Hold) != 0)
            {
                return Send("{Alt}");
            }

            _action |= _Action.WithAlt;
            return this;
        }
        public Controller Shift()
        {
            if ((_action & _Action.Hold) != 0)
            {
                return Send("{Shift}");
            }

            _action |= _Action.WithShift;
            return this;
        }
        public Controller Hold {
            get {
                _action = _Action.Hold;
                return this;
            }
        }
        public Controller Release {
            get {
                _action = _Action.Release;
                return this;
            }
        }
    }
}
