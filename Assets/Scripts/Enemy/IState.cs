using UnityEngine;

public interface IState 
{
    void Enter(); //Khi vào State
    void Execute(); // Khi đang ở trong State
    void Exit(); // Khi rời khỏi State
}
