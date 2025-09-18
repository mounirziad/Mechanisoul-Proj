using UnityEngine;

public interface IActionStrategy
{
    bool CanPerform { get; } //Can we execute the strategy
    bool Complete { get; } //Is the strategy finished

    void Start() //run everytime we want to execute a strategy
    {
        //interface needs them but not using at the moment
    }

    void Update(float deltaTime) //update frame by frame using delta time
    {
        //interface needs them but not using at the moment
    }

    void Stop() //stopping strategy
    {
        //interface needs them but not using at the moment
    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true; //Agent can always idle
    public bool Complete { get; private set; } //set complete after timer

    readonly CountdownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}
