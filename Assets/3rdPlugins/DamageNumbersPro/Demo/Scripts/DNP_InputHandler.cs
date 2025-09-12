using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && DNP_NewInputSystem
using UnityEngine.InputSystem;

namespace DamageNumbersPro.Demo
{
    public static class DNP_InputHandler
    {
        //Directions:
        public static bool GetRight()
        {
            if(Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.D].isPressed || Keyboard.current[Key.RightArrow].isPressed;
            }
        }
        public static bool GetLeft()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.A].isPressed || Keyboard.current[Key.LeftArrow].isPressed;
            }
        }
        public static bool GetBack()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.S].isPressed || Keyboard.current[Key.DownArrow].isPressed;
            }
        }
        public static bool GetForward()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.W].isPressed || Keyboard.current[Key.UpArrow].isPressed;
            }
        }

        //Vertical:
        public static bool GetJump()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.Space].isPressed;
            }
        }
        public static bool GetUp()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.E].isPressed || Keyboard.current[Key.Space].isPressed;
            }
        }
        public static bool GetDown()
        {
            if (Keyboard.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.Q].isPressed || Keyboard.current[Key.LeftShift].isPressed;
            }
        }

        //Mouse:
        public static bool GetLeftClick()
        {
            if (Mouse.current == null)
            {
                return false;
            }
            else
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }
        }
        public static bool GetLeftHeld()
        {
            if (Mouse.current == null)
            {
                return false;
            }
            else
            {
                return Mouse.current.leftButton.isPressed;
            }
        }
        public static bool GetRightClick()
        {
            if (Mouse.current == null)
            {
                return false;
            }
            else
            {
                return Mouse.current.rightButton.wasPressedThisFrame;
            }
        }
        public static bool GetRightHeld()
        {
            if (Mouse.current == null)
            {
                return false;
            }
            else
            {
                return Mouse.current.rightButton.isPressed;
            }
        }
        public static Vector2 GetMouseDelta()
        {
            if (Mouse.current == null)
            {
                return Vector2.zero;
            }
            else
            {
                return 100f * Mouse.current.delta.ReadValue() / (float) Screen.height;
            }
        }
        public static float GetMouseScroll()
        {
            if (Mouse.current == null)
            {
                return 0;
            }
            else
            {
                return Mouse.current.scroll.ReadValue().y;
            }
        }

        //Escape:
        public static bool GetEscape()
        {
            if (Mouse.current == null)
            {
                return false;
            }
            else
            {
                return Keyboard.current[Key.Escape].wasPressedThisFrame;
            }
        }
    }
}
#else
namespace DamageNumbersPro.Demo
{
    public static class DNP_InputHandler
    {
        //Directions:
        public static bool GetRight()
        {
            return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        }
        public static bool GetLeft()
        {
            return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        }
        public static bool GetBack()
        {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }
        public static bool GetForward()
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        //Vertical:
        public static bool GetJump()
        {
            return Input.GetKey(KeyCode.Space);
        }
        public static bool GetUp()
        {
            return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
        }
        public static bool GetDown()
        {
            return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftShift);
        }

        //Other:
        public static bool GetLeftClick()
        {
            return Input.GetMouseButtonDown(0);
        }
        public static bool GetLeftHeld()
        {
            return Input.GetMouseButton(0);
        }
        public static bool GetRightClick()
        {
            return Input.GetMouseButtonDown(1);
        }
        public static bool GetRightHeld()
        {
            return Input.GetMouseButton(1);
        }
        public static Vector2 GetMouseDelta()
        {
            return new Vector2(Input.GetAxisRaw("Mouse X") * 2f, Input.GetAxisRaw("Mouse Y") * 2f);
        }
        public static float GetMouseScroll()
        {
            return Input.mouseScrollDelta.y;
        }

        //Escape:
        public static bool GetEscape()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.I);
        }
    }
}
#endif


