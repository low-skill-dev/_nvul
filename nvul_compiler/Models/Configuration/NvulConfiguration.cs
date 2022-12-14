using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	public class NvulConfiguration
	{
		public IList<NvulFunction> NvulFunctions { get; set; }
		public IList<NvulImplicit> Implicits { get; set; }
		public IList<NvulKeyword> Keywords { get; set; }
		public IList<NvulTranslation> Translations { get; set; }
		public IList<NvulOperator> Operators { get; set; }

		public NvulConfiguration(IList<NvulFunction> nvulFunctions, IList<NvulImplicit> implicits, IList<NvulKeyword> keywords, IList<NvulTranslation> translations, IList<NvulOperator> operators)
		{
			this.NvulFunctions = nvulFunctions;
			this.Implicits = implicits;
			this.Keywords = keywords;
			this.Translations = translations;
			this.Operators = operators;
		}
	}
}
