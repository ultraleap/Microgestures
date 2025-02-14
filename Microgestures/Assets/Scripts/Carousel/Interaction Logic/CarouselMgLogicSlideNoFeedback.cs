using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimatedProgressQueue))]
public class CarouselMgLogicSlideNoFeedback : CarouselMgLogicSlide
{
    private AnimatedProgressQueue _queue;

    protected override void Awake()
    {
        _queue = GetComponent<AnimatedProgressQueue>();
        _queue.Advance += delegate (SwipeDirection d) { Advance?.Invoke(d); };
        _queue.Progress += delegate (float p) { Progress?.Invoke(p); };

        base.Awake();
    }

    protected override void DoAdvance(SwipeDirection direction)
    {
        _queue.AddDirectionToQueue(direction);
    }

    protected new void Update()
    {
        _realValue = _swipeProgressReal;
    }
}