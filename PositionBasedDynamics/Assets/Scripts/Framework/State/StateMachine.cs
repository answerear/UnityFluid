namespace Framework
{
    public class StateMachine<T>
    {
        protected T mOwner;
        protected IState<T> mCurrentState;
        protected IState<T> mPreviousState;
        
        public IState<T> CurrentState
        {
            get { return mCurrentState; }
            set { mCurrentState = value; }
        }

        public IState<T> PreviousState
        {
            get { return mPreviousState; }
            set { mPreviousState = value; }
        }

        public StateMachine(T owner)
        {
            mOwner = owner;
            mCurrentState = null;
            mPreviousState = null;
        }

        /// <summary>
        /// 改变到新状态
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(IState<T> newState)
        {
            if(newState != null)
            {
                mPreviousState = mCurrentState;
                if (mCurrentState != null)
                    mCurrentState.Exit(mOwner);
                mCurrentState = newState;
                mCurrentState.Enter(mOwner);
            }
        }

        /// <summary>
        /// 改变状态回到前一个状态
        /// </summary>
        public void RevertToPreviousState()
        {
            ChangeState(mPreviousState);
        }

        public void DoUpdate(float dt)
        {
            if (mCurrentState != null)
                mCurrentState.DoUpdate(mOwner, dt);
        }
    }
}
