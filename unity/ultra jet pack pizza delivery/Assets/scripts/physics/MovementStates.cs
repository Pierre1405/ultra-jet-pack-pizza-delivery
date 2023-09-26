using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.scripts.physics
{

    class MovementStateUpdater<U, T>
    {

        private U previousState;
        private U currentState;
        private U nextState;
        private readonly Func<MovementStateUpdater<U, T>, T> toTransition;

        public MovementStateUpdater(U currentState, Func<MovementStateUpdater<U, T>, T> toTransition)
        {
            this.currentState = currentState;
            this.toTransition = toTransition;
            this.previousState = currentState;
        }

        public void update(U nextState)
        {
            this.nextState = nextState;
        }

        public T apply()
        {
            previousState = currentState;
            currentState = nextState;
            return transitionState();
        }

        public T transitionState()
        {
            return toTransition.Invoke(this);
        }

        public U getCurrent()
        {
            return currentState;
        }

        public U getPrevious()
        {
            return previousState;
        }

    }
}
