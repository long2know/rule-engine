namespace RuleEngine.Interfaces
{
	public interface IParser
	{
		/// <summary>
		/// Parses a list of tokens using a simple grammar
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		object Parse<T>(string expression, T model);

		object Parse(string expression);

		V Parse<T, V>(string expression, T model);

		V Parse<V>(string expression);
	}
}
