namespace Fubaza.Application.Core.Contracts
{
    public interface IResult
    {
        List<string> Messages { get; set; }

        bool Succeeded { get; set; }
        public object[]? Arguments { get; set; }

    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}
