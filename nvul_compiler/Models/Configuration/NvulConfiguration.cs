using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulConfiguration
	{
		public IList<NvulFunction> NvulFunctions { get; set; }
		public IList<NvulImplicit> Implicits { get; set; }
		public IList<NvulKeyword> Keywords { get; set; }
		public IList<NvulTranslation> Translations { get; set; }

		public NvulConfiguration(IList<NvulFunction> nvulFunctions, IList<NvulImplicit> implicits, IList<NvulKeyword> keywords, IList<NvulTranslation> translations)
		{
			this.NvulFunctions = nvulFunctions;
			this.Implicits = implicits;
			this.Keywords = keywords;
			this.Translations = translations;
		}
	}
}
