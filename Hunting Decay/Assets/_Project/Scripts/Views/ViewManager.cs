using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>Turns one view on and the others off, and passes the day state down to every view.</summary>
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] Scope scope;
        [SerializeField] List<View> views = new();

        IDayStateListener[] listeners;

        public View Current { get; private set; }

        void Awake()
        {
            listeners = views.SelectMany(view => view.GetComponentsInChildren<IDayStateListener>(true)).ToArray();
        }

        public void Show(View view)
        {
            foreach (View each in views)
                each.gameObject.SetActive(each == view);
            Current = view;
            scope.SetBackground(view.Background);
        }

        public void ApplyState(int day, int missedAnomalies)
        {
            foreach (IDayStateListener listener in listeners)
                listener.OnDayState(day, missedAnomalies);
        }
    }
}
