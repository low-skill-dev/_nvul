using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulTranslation
	{
		public string Word { get; set; }
		public string TranslateTo { get; set; }

		public NvulTranslation(string word, string translateTo)
		{
			this.Word = word;
			this.TranslateTo = translateTo;
		}
	}
}
