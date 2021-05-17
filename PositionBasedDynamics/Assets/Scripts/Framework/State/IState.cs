namespace Framework
{
    public interface IState<T>
    {
        void Enter(T owner);
        void Exit(T owner);
        void DoUpdate(T owner, float dt);
    }
}
