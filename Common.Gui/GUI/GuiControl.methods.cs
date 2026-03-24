using System;
using System.Collections.Generic;
using System.Diagnostics;

using Common.Maths;

namespace Common.Gui
{
    public abstract partial class GuiControl
    {
        public static double performance_GetTopMost;
        public static double performance_PreCalculate;
        public static double performance_SetTopMost;

        public override void Update()
        {
            if (Elements!=null) foreach(var element in Elements) element.Update();
            if (Elements_ != null) Elements_.Update();
        }

        /// <summary>
        /// Get the top most selected element. The tree must be updated with depth and absolute rectangle.
        /// </summary>
        /// <param name="abs_x">mouse coordinate in absolute coordinate</param>
        /// <param name="abs_y">mouse coordinate in absolute coordinate</param>
        /// <param name="node">the element to start, usualy the root of tree</param>
        /// <param name="clipmode">if not <see cref="GuiClipMode.None"/>consider the children always inside the parents, can improve performance</param>
        public static GuiControl GetTopMost(Vector2i absolute, GuiControl node)
        {
            //The tree must be updated with depth and absolute rectangle.
            node.Manager.DoUpdateJob();

            int maxdepth = 0;
            GuiControl topmost = null;
#if DEBUG
            var t0 = TimerTick.Ticks;
#endif
            //TODO : reduce the number of iterations when searching using clip region
            foreach (var element in GuiTreeTraversal.Forward(node))
                if (element.depth > maxdepth && element.ContainMouse(absolute))
                {
                    maxdepth = element.depth;
                    topmost = element;
                }

            /////////////////// not work for GuiButton in GuiComboBox ?????
            /*
            if (!root.Manager.UseClipping)
            {
                
                foreach (var element in GuiTreeTraversal.Forward(root))
                    if (element.depth > maxdepth && element.ContainMouse(absolute))
                    {
                        maxdepth = element.depth;
                        topmost = element;
                    }
            }
            else
            {
                foreach (var element in GuiTreeTraversal.ForwardSkipOutOfParentBound(root, absolute))
                    if (element.depth > maxdepth)
                    {
                        maxdepth = element.depth;
                        topmost = element;
                    }
            }
            */
#if DEBUG
            var t1 = TimerTick.Ticks;
            performance_GetTopMost = TimerTick.GetMSFromTick(t1 - t0);
            //Debug.Print("GetTopMost " + performance_GetTopMost);
#endif
            return topmost;
        }

        /// <summary>
        /// Fix the depth values, recalculate all clip rectangles
        /// </summary>
        /// <param name="update">update the job done</param>
        /// <returns>return the total number of elements</returns>
        internal static int PreCalculate(GuiControl node, ref GuiUpdate update)
        {
            if (node == null) return 0;
#if DEBUG
            var t0 = TimerTick.Ticks;
#endif
            bool calculate_clip = update.HasFlag(GuiUpdate.Clip);
            
            int lastdepth = 0;
            if (node.UseClipping) node.m_cliprectangle = node.m_globalrect;
            node.depth = lastdepth++;

            if (calculate_clip)
                foreach (var control in GuiTreeTraversal.ForwardSkipRoot(node))
                {
                    //children inherit the first clip rectangle
                    //element.ClipParent can't be null
                    control.m_cliprectangle = control.ClipParent.m_cliprectangle;
                    if (control.UseClipping)
                        Rectangle4i.Intersect(control.m_cliprectangle, control.m_globalrect, out control.m_cliprectangle);
                    else
                        control.m_cliprectangle = control.Destination;

                    //update depth always
                    control.depth = lastdepth++;
                }
            else
                //only update depth
                foreach (var element in GuiTreeTraversal.ForwardSkipRoot(node))
                    element.depth = lastdepth++;


            //remove the job that are done
            //update &= ~(GuiUpdate.Depth | GuiUpdate.Clip);
            update = GuiUpdate.None;

#if DEBUG
            var t1 = TimerTick.Ticks;
            performance_PreCalculate = TimerTick.GetMSFromTick(t1 - t0);
            //Debug.Print("PreCalculate " + performance_PreCalculate);
#endif
            return lastdepth;
        }

        /// <summary>
        /// Check if mouse is inside this guielement
        /// </summary>
        /// <remarks>
        /// the collision can be changed in case the simple rectangle is not enough
        /// </remarks>
        /// <param name="abs_x">absolute x (relative to root)</param>
        /// <param name="abs_y">absolute y (relative to root)</param>
        public virtual bool ContainMouse(int abs_x, int abs_y) => m_globalrect.Contain(abs_x, abs_y);
        
        public bool ContainMouse(Vector2i abs) => ContainMouse(abs.x, abs.y);

        /// <summary>
        /// remove this instance from elements tree and all its children
        /// </summary>
        internal static void Remove(GuiControl element)
        {
            if (element.HasParent)
            {
                element.Parent.children.Remove(element);
                element.Destroy();
            }
        }


        /// <summary>
        /// Mark the element on the top of depth order assigning a larger depth value.
        /// Children will be always on top, selecting a element automatically selects all relatives.
        /// It's not necessary recalculate all depths before any new "SetToMost"
        /// </summary>
        public static void SetTopMost(GuiControl selected)
        {
#if DEBUG
            var t0 = TimerTick.Ticks;
            recursiveSetTopMost(selected);
            var t1 = TimerTick.Ticks;
            performance_SetTopMost = TimerTick.GetMSFromTick(t1 - t0);
            //Debug.Print("SetTopMost " + performance_SetTopMost);
#else
            recursiveSetTopMost(selected);
#endif
        }

        static void recursiveSetTopMost(GuiControl selected)
        {
            if (selected.HasParent)
            {
                int maxdepth = 0;
                foreach (var child in selected.Parent.children) maxdepth = Math.Max(maxdepth, child.depth);
                selected.Depth = maxdepth + 1;
                recursiveSetTopMost(selected.Parent);
            }
            selected.Manager.updatePending |= GuiUpdate.Depth;
        }

        void Destroy()
        {
            if (Manager == null) Debug.Print("already destroyed");
            
            if (HasChildren)
                foreach (var child in ChildrenDepthSorted) child.Destroy();

            children = null;
            depth = -1;
            Parent = null;
            Manager = null;
            IsEnabled = true;
        }
    }
}
