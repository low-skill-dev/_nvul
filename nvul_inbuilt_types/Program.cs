using System.Text;

class IntegerMatrix
{
	private long[,] _source;
	private IntegerMatrix(long rows, long cols)
	{
		this._source = new long[rows, cols];
	}

	public static IntegerMatrix FactoryCreate(long rows, long cols)
	{
		return new IntegerMatrix(rows, cols);
	}

	public static IntegerMatrix InputCreate(long rows, int cols)
	{
		IntegerMatrix result = new IntegerMatrix(rows, cols);
		for (int r = 0; r < result._source.GetLength(0); r++)
		{
			for (int c = 0; c < result._source.GetLength(1); c++)
			{
				Console.Write($"[{r},{c}]=");
				result.SetValue(long.Parse(Console.ReadLine() ?? "0"), r, c);
			}
		}
		return result;
	}

	public void SetValue(long val, long row, long col) => this._source.SetValue(val, row, col);
	public override string ToString()
	{
		int maxLen =0;
		// I dont have time to optimize
		foreach (long i in _source) if (i.ToString().Length > maxLen) maxLen = i.ToString().Length;

		StringBuilder sb = new(maxLen/2*_source.Length);

		for(int r = 0; r < _source.GetLength(0); r++)
		{
			for(int c = 0; c < _source.GetLength(1); c++)
			{
				var str = _source[r, c].ToString();
				sb.Append(new string(' ',maxLen-str.Length+1)+str);
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}
}
class Program
{
	public static void Main()
	{
		IntegerMatrix myMtr;
		myMtr = IntegerMatrix.InputCreate(2, 2);
		long c1;
		c1 = 1;
		while ((c1 > 0))
		{
			myMtr.SetValue(c1, 0, 0);
		};
		Console.WriteLine(myMtr);
	}
}
