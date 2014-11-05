using System;

namespace FlickerBox.Events
{
    public static class EventExtentionsMethods
    {
        static public void RaiseEvent(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
                handler(sender, e);
        }

        static public void RaiseEvent<T>(this EventHandler<T> handler, object sender, T e)
        {
            if (handler != null)
                handler(sender, e);
        }
    }
}
