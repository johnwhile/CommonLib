using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Inputs
{
    public interface IInputDevice
    {
        /// <summary>
        /// Read the keys state, return true if success
        /// </summary>
        bool Update();
        /// <summary>
        /// Stop when application is not focused
        /// </summary>
        void StopAcquiring();
        /// <summary>
        /// Enable when application is focused
        /// </summary>
        void EnableAcquiring();
        /// <summary>
        /// Get true is EnableAcquiring success
        /// </summary>
        bool IsAcquired { get; }
    }


    public interface IMouse : IInputDevice
    {
        /// <summary>
        /// Return the mouse state, require Update to copy the last mouse state
        /// </summary>
        MyMouseState GetMouseState { get; }

        event MyMouseEventHandler OnMouseDoubleClick;
        event MyMouseEventHandler OnMouseDown;
        event MyMouseEventHandler OnMouseUp;
        event MyMouseEventHandler OnMouseMove;
        event MyMouseEventHandler OnMouseWheel;
    }

    public interface IKeyboard : IInputDevice
    {
        MyKeyboardState GetKyboardState { get; }

        event MyKeyEventHandler OnKeyStateChanged;
        event MyKeyEventHandler OnKeyDown;
        event MyKeyEventHandler OnKeyUp;
    }

}
