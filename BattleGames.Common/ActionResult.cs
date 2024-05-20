using System;

namespace Stip.BattleGames.Common;

public class ActionResult<T> : ActionResult
{
    public new static ActionResult<T> Success => new(true);
    public new static ActionResult<T> Failure => new(false);

    public new static ActionResult<T> FromSuccessState(bool isSuccess)
        => isSuccess
            ? Success
            : Failure;

    public T Data { get; }

    private ActionResult(bool isSuccess)
        : base(isSuccess) { }

    public ActionResult(bool isSuccess, string message)
        : base(isSuccess, message) { }

    public ActionResult(string message, Exception e)
        : base(message, e) { }

    public ActionResult(string message, ActionResult innerResult)
        : base(message, innerResult) { }

    public ActionResult(bool isSuccess, T response)
        : base(isSuccess)
    {
        Data = response;
    }

    public ActionResult(bool isSuccess, string message, T response)
        : base(isSuccess, message)
    {
        Data = response;
    }

    public ActionResult(string message, Exception e, T response)
        : base(message, e)
    {
        Data = response;
    }

    public ActionResult(ActionResult serviceResult)
        : base(serviceResult.IsSuccess, serviceResult.Message) { }

    public ActionResult(ActionResult serviceResult, T response)
        : this(serviceResult)
    {
        Data = response;
    }

    public ActionResult(T response)
        : base(true)
    {
        Data = response;
    }
}

public class ActionResult
{
    public static readonly ActionResult Success = new(true);
    public static readonly ActionResult Failure = new(false);
    public static readonly ActionResult<bool> TrueBoolean = new(true);
    public static readonly ActionResult<bool> FalseBoolean = new(false);

    public static ActionResult FromSuccessState(bool isSuccess)
        => isSuccess
            ? Success
            : Failure;

    public static ActionResult<bool> FromBoolean(bool response)
        => response
            ? TrueBoolean
            : FalseBoolean;

    public bool IsSuccess { get; }
    public string Message { get; }

    protected ActionResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
        Message = string.Empty;
    }

    public ActionResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public ActionResult(string message, Exception e)
    {
        IsSuccess = false;
        Message = $"{message}{Environment.NewLine}{e.Message}";
    }

    public ActionResult(string message, ActionResult innerResult)
    {
        IsSuccess = false;
        Message = $"{message}{Environment.NewLine}{innerResult.Message}";
    }
}
