/*
 * Created by SharpDevelop.
 * User: Шаркади Андрей
 * Date: 22.04.2010, 07.09.2010.
 */
using System;
using System.Collections;

namespace FuzzyLogics
{
	/* =======================================
	 * Класс <FuzzySet>	 
	 * НЕЧЕТКОЕ МНОЖЕСТВО	 
	 * Представляет собой нечеткое множество, как таковое.
	 * Содержит элементы множества как пары Х-вероятность
	 * (X неповторяется).
	======================================== */
	public class FuzzySet
	{
		/* Имя множества и сортированный список элементов множества.
		 * FuzzySetElements: 
		 * 	Key - значение оси X;
		 * 	Value - значение вероятности по оси Y, соответсвует Key;
		*/
		internal string SetName;
		internal SortedList FuzzySetElements = new SortedList();

		public const int MethodAnd = 1;
		public const int MethodOr = 2;
		public const int MethodNot = 3;		
		
		// Конструкторы
		internal FuzzySet () { SetName = ""; FuzzySetElements  = new SortedList(); }
		
		public FuzzySet (string name) { SetName = name; FuzzySetElements = new SortedList(); }
		
		public FuzzySet (string name, SortedList setX_Probability) 
		{ SetName = name; FuzzySetElements = setX_Probability; }
		
		public FuzzySet (string name, aLine.aPoint[] points)
		{
			SetName = name;
			foreach (aLine.aPoint point in points) Add(point.x, point.y);
		}
		public FuzzySet (string name, params double[] assoc) 	// Четные элементы - Key, нечетные - Value
		{
			FuzzySetElements = new SortedList();
			SetName = name;
			bool even = false;
			object key = null;
			foreach (Double elem in assoc)
			{
				if (even == false) {
					key = elem; even = true ;
				} else {
					Add(key, elem);
					key = null; even = false;
			}}
		}

		public static FuzzySet operator & (FuzzySet fset1, FuzzySet fset2)
		{ return CombineSets(MethodAnd, fset1, fset2); }
		
		public static FuzzySet operator | (FuzzySet fset1, FuzzySet fset2)
		{ return CombineSets(MethodOr, fset1, fset2); }
		
		public static bool operator true (FuzzySet fset) 
		{ return (fset.FuzzySetElements.Count > 0) ? true : false; }
		
		public static bool operator false (FuzzySet fset)
		{ return (fset.FuzzySetElements.Count == 0) ? true : false; }
		
		public void Add (object x, object probability) 
		{ if (!FuzzySetElements.ContainsKey(x)) FuzzySetElements.Add(x, probability); }
		
		public void Add (FuzzySet sourceSet) 
		{
			foreach (DictionaryEntry elem in sourceSet.FuzzySetElements)
			{ Add(elem.Key, elem.Value); }
		}
		
		public void Remove (Double x) { FuzzySetElements.Remove(x); }
		
		public void Remove () { FuzzySetElements.Clear(); }
		
		public Array GetXset()
		{
			Array results = Array.CreateInstance(typeof(double), 
			                                     FuzzySetElements.Keys.Count);
			FuzzySetElements.Keys.CopyTo(results, 0);
			return results;
		}
		
		public Array GetProbabilitySet() 
		{ 
			Array results = Array.CreateInstance(typeof(double),
			                                     FuzzySetElements.Values.Count);
			FuzzySetElements.Values.CopyTo(results, 0);
			return results;
		}
		
		public bool EqualX (FuzzySet sourceX)
		{
			IList ElemX1, ElemX2;
			ElemX1 = this.FuzzySetElements.GetKeyList();
			ElemX2 = sourceX.FuzzySetElements.GetKeyList();
			return IList.Equals(ElemX1, ElemX2);
		}
		
		internal SortedList GetFuzzySetElems ()
		{ return FuzzySetElements; }
		
		public object GetXvalue (object X)
		{ return FuzzySetElements[X]; }
		
		public override string ToString()
		{
			string results = "";
			foreach (DictionaryEntry elem in this.FuzzySetElements)
				results += "(" + elem.Key + ";" + elem.Value + ")-";
			if (results.Length == 0) results =  "0.";
			return results.Substring(0, results.Length-1);
		}

		// КОМБИНИРОВАТЬ МНОЖЕСТВА: комбинирует
		// множества некоторым методом - И, ИЛИ, НЕ.
		// 	1. получить множество линий из каждого нечет-
		// кого множетсва;
		//	2. получить нечеткое множество точек пере-
		// сечения двух множеств линий;
		//	3. одним из логических методов нечеткого срав-
		// нения получить результирующее множество;
		//	4. вернуть новую л. п., область определения
		// которой - результирующее множество. Также
		// добавляется в массив нечетких множеств все
		// сравниваемые множества.		
		private static FuzzySet CombineSets (int method, FuzzySet fset1, FuzzySet fset2)
		{
			SortedList set1 = fset1.FuzzySetElements;
			SortedList set2 = fset2.FuzzySetElements;
			FuzzySet results = new FuzzySet();
			SortedList resultset = new SortedList();
			ArrayList lines1 = new ArrayList(), lines2 = new ArrayList();
			lines1 = aLine.GetLines(set1); lines2 = aLine.GetLines(set2);
			SortedList intrset = aLine.GetIntrPointSet(lines1, lines2);
			switch (method) {
				case MethodAnd:
					resultset = FuzzyAndOr(method, lines1, lines2, intrset, set1, set2);
					break;
				case MethodOr:
					resultset = FuzzyAndOr(method, lines1, lines2, intrset, set1, set2);
					break;
			}
			results.FuzzySetElements = resultset;
			return results;			
		}
	
		private static SortedList FuzzyAndOr (int method, ArrayList lines1, ArrayList lines2, SortedList intrset, SortedList set1, SortedList set2)
		{
			SortedList resultset =  intrset;
			ArrayList lineset1 = new ArrayList(), lineset2 = new ArrayList();
			SortedList targ_set1 = new SortedList(), targ_set2 = new SortedList();
			if (GetSetMaxValKey(set1) >= GetSetMaxValKey(set2)) {
				lineset1 = lines1; 
				lineset2 = lines2;
				targ_set1 = set2; 
				targ_set2 = set1;
			} else {
				lineset1 = lines2; 
				lineset2 = lines1;
				targ_set1 = set1; 
				targ_set2 = set2;
			}
			aLine extremes = aLine.GetExtremes(method, aLine.ExtractRangeFromSet(lines1), aLine.ExtractRangeFromSet(lines2));
			foreach (aLine line in lineset1)
			{
				foreach (DictionaryEntry elem in targ_set1) 
				{
					if (!(((double)elem.Key >= extremes.x1) && ((double)elem.Key <= extremes.x2))) continue;
					if ((method == MethodAnd) && aLine.PointPossitionInspect(line, new aLine.aPoint((double)elem.Key, 
					                                                                                (double)elem.Value), aLine.PointBelow, true))
						{  if (!resultset.ContainsKey(elem.Key)) resultset.Add(elem.Key, elem.Value); }
					else  if ((method == MethodOr) && aLine.PointPossitionInspect(line, new aLine.aPoint((double)elem.Key,
					                                                                                     (double)elem.Value), aLine.PointAbove, true))
						{ if (!resultset.ContainsKey(elem.Key)) resultset.Add(elem.Key, elem.Value); }
				}
			}
			foreach (aLine line in lineset2)
			{
				foreach (DictionaryEntry elem in targ_set2)
				{
					if (!(((double)elem.Key >= extremes.x1) && ((double)elem.Key <= extremes.x2))) continue;
					if (aLine.PointPossitionInspect(line, new aLine.aPoint((double)elem.Key, (double)elem.Value), aLine.PointBelow, true))
						{ if (!resultset.ContainsKey(elem.Key)) resultset.Add(elem.Key, elem.Value); }
					else  if ((method == MethodOr) && aLine.PointPossitionInspect(line, new aLine.aPoint((double)elem.Key, 
					                                                                                     (double)elem.Value), aLine.PointAbove, true))
						{ if (!resultset.ContainsKey(elem.Key)) resultset.Add(elem.Key, elem.Value); }
				}
			}
			if (!resultset.ContainsKey(extremes.x1)) resultset.Add(extremes.x1, 0);
			if (!resultset.ContainsKey(extremes.x2)) resultset.Add(extremes.x2, 0);
			return resultset;
		}
	
		private static double GetSetMaxValKey (SortedList set)
		{
			double key = (double)set.GetByIndex(0);
			foreach (DictionaryEntry  elem in set)
			{ if ((double)elem.Value > key) key = (double)elem.Value; }
			return key;
		}
	}

	
	/* =======================================
	 * Класс <LinguisticVariable>
	 * ЛИНГВИСТИЧЕСКАЯ ПЕРЕМЕННАЯ
	 * Содержит в себе нечеткое множество, являющееся
	 * обалстью определения самой л. п. Также включает
	 * массив всех нечетких множеств (термов), их коли-
	 * чество может быть любым, но не менее одного
	 * (дублирует обасть определения самой л. п.).
	======================================= */
	public class LinguisticVariable
	{
		internal  ArrayList LingVarTerms;
		internal  FuzzySet LingVarSet;
//		private string LingVarName;
		
		private void InitLinguisticVariable ()
		{ this.LingVarSet = new FuzzySet(); this.LingVarTerms = new ArrayList(); }
		
		public LinguisticVariable () 
		{ InitLinguisticVariable(); }
		
		public LinguisticVariable (FuzzySet fset) {
			InitLinguisticVariable();
			this.LingVarTerms.Add(fset);
			this.LingVarSet = fset;
			this.LingVarSet.SetName = fset.SetName;
		}
		
		public static LinguisticVariable operator & (LinguisticVariable lvar1, LinguisticVariable lvar2) 
		{ return CombineSets(lvar1.LingVarSet & lvar2.LingVarSet, lvar1, lvar2); }
		
		public static LinguisticVariable operator | (LinguisticVariable lvar1, LinguisticVariable lvar2)
		{ return CombineSets(lvar1.LingVarSet | lvar2.LingVarSet, lvar1, lvar2); }
		
		public static bool operator true (LinguisticVariable lvar) 
		{ return (lvar.LingVarSet.FuzzySetElements.Count > 0) ? true : false; }
		
		public static bool operator false (LinguisticVariable lvar)
		{ return (lvar.LingVarSet.FuzzySetElements.Count == 0) ? true : false; }
			
		// ОБЪЕДИНИТЬ: Объединяет несколько сортированных
		// списков в один, используется для объединения элемен-
		// тов нечетких множеств.
		private static SortedList Merge (params SortedList[] lists)
		{
			SortedList results = new SortedList();
			foreach (SortedList elem1 in lists)
			{ foreach (DictionaryEntry elem2 in elem1)
				{
					if (!results.Contains(elem2.Key)) results.Add(elem2.Key, elem2.Value);
				}}
			return results;
		}
			
		private void AddToSetOfFuzzySets (params FuzzySet[] sets)
		{ foreach (FuzzySet elem in sets) { this.LingVarTerms.Add(elem); } }
		
		private void AddToSetOfFuzzySets (params ArrayList[] lists)
		{ 
			foreach (ArrayList elem1 in lists)
			{ foreach (FuzzySet elem2 in elem1)
				{
					this.LingVarTerms.Add(elem2);
				}}
		}
		
		private static LinguisticVariable CombineSets (FuzzySet resultset, LinguisticVariable lvar1, LinguisticVariable lvar2)
		{
			LinguisticVariable results = new LinguisticVariable();
			results.AddToSetOfFuzzySets(lvar1.LingVarSet, lvar1.LingVarSet);
			results.AddToSetOfFuzzySets(lvar1.LingVarTerms, lvar2.LingVarTerms);
			results.LingVarSet.FuzzySetElements = resultset.FuzzySetElements;
			return results;
		}
			
		private static ArrayList AddToArray (params ArrayList[] elems)
		{
			ArrayList results = new ArrayList();
			foreach (ArrayList  elem1 in elems)
			{ foreach (aLine elem2 in elem1) { results.Add(elem2); }}
			return results;
		}
		
		public void Clear() 
		{
			LingVarTerms.Clear();
			LingVarTerms.Add(new FuzzySet(LingVarSet.SetName, LingVarSet.FuzzySetElements));
		}
		
		public override string ToString() { return LingVarSet.ToString(); }
	}
	
	
	/* ======================================
	 * Класс <ALine>
	 * ЛИНИЯ
	 * Основное назначение - разбитие множества на линии
	 * и поиск точек их пересечения, обозначение области
	 * определения двух множеств.
	======================================= */
	public class aLine
	{
		public  const int PointAbove = 1;
		public  const int PointBelow = 2;
		public double x1, x2, y1, y2;
			
		public aLine()
		{ x1 = 0.0; y1 = 0.0; x2= 0.0; y2 = 0.0; }
			
		public aLine(double X1, double X2, double Y1, double Y2)
		{ x1 = X1 ; y1 = Y1; x2= X2; y2 = Y2; }
			
		public override string ToString()
		{ return "(" + x1.ToString() + ";" + y1.ToString() + 
				")-(" + x2.ToString() + ";" + y2.ToString() + ")"; }
		
		/* ------------------------------------------------------------------------------
	 	* Структура <Point>
	 	* ТОЧКА
	 	* Точку можно использовать для самых разных целей,
	 	* преимущественно в целях определения области 
	 	* пересечения двух множеств (в графическом виде).
		--------------------------------------------------------------------------------- */
		public struct aPoint
		{ 
			public double x, y; internal bool success; 
			public aPoint(double X, double Y) 
			{ x = X; y = Y; success= false; }
			public override string ToString()
			{ return "(" + x + ";" + y + ")"; }
		}			
			
		// ПОЛУЧИТЬ ЛИНИИ: Разбивает множество на линии
		public static ArrayList GetLines (SortedList set1)	
		{
			aLine aline = new aLine();
			ArrayList results = new ArrayList();
			foreach (DictionaryEntry elem in set1)
			{
				if (aline.x1 == 0.0 && aline.y1 == 0.0) {
					aline.x1 = (double)elem.Key;
					aline.y1 = (double)elem.Value;
					//if (aline.y1 > 0.0) results.Add(new ALine(aline.x1, aline.x1, 0, aline.y1));
				}
					else
				{
					aline.x2 = (double)elem.Key;
					aline.y2 = (double)elem.Value;
					results.Add(new aLine(aline.x1, aline.x2, aline.y1, aline.y2));
					aline.x1 = aline.x2; aline.y1 = aline.y2;
				}				
			}
			//if (aline.y2 > 0.0) results.Add(new ALine(aline.x2, aline.x2, aline.y2, 0));
			return results;
		}

		// ИССЛЕДОВАТЬ ПОЗИЦИЮ ТОЧКИ: позволяет
		// узнать расположение точки относительно отрезка
		// и осуществить проверку, находится ли точка в 
		// границах отрезка.
		public static bool PointPossitionInspect (aLine line, aLine.aPoint point, int WhereShouldBe, bool BoundaryCheck)
		{
			if (BoundaryCheck && !((point.x >= line.x1) && (point.x <= line.x2))) return false;
			double position = (line.x2-line.x1)*(point.y-line.y1)-(line.y2-line.y1)*(point.x-line.x1);
			if (position > 0) return WhereShouldBe == aLine.PointAbove ? true : false;	// точка слева от отрезка
			if (position < 0) return WhereShouldBe == aLine.PointBelow ? true : false;	// точка справа от отрезка
			return false;
		}
		
		// ПОЛУЧИТЬ ЭКСТРЕМЫ: возвращает крайние точки
		// как область определения двух множеств линий в
		// зависимости от применяемого к ним логического метода
		// комбинирования.
		// TODO: GetExtremes - проверить работоспособность, получено эмпирическим путем.
		public static aLine GetExtremes (int method, aLine line1, aLine line2)
		{
			aLine results = new aLine(0, 0, 0, 0);
			if ((line1.x1 >= line2.x1) && (line1.x2 <= line2.x2)) 	// первое множество входит во второе
			{ 
				results.x1 = (method == FuzzySet.MethodAnd ? line1.x1 : line2.x1);
				results.x2 = (method == FuzzySet.MethodAnd ? line1.x2 : line2.x2); 
				return results;
			}
			if ((line2.x1 >= line1.x1) && (line2.x2 <= line1.x2))	// второе множество входит в первое
			{
				results.x1 = (method == FuzzySet.MethodAnd ?  line2.x1 : line1.x1);
				results.x2 = (method == FuzzySet.MethodAnd ?  line2.x2 : line1.x2);
				return results;
			}
			if (Math.Abs(line1.x1-line2.x2) < Math.Abs(line1.x2-line2.x1))	// в ином случае определим линии их пересечения
			{	
				results.x1 = (method == FuzzySet.MethodAnd ? line1.x1 : line1.x2);
				results.x2 = (method == FuzzySet.MethodAnd ? line2.x2 : line2.x1);
			} else {
				results.x1 = (method == FuzzySet.MethodAnd ? line2.x1 : line1.x1);
				results.x2 = (method == FuzzySet.MethodAnd ? line1.x2 : line2.x2);
			}
			return results;
		}
		
		public static aLine ExtractRangeFromSet (ArrayList lines) 
		{
			aLine first = (aLine)lines[0], last = (aLine)lines[lines.Count-1];
			return new aLine(first.x1, last.x2, 0, 0);
		}
		
		// ТОЧКА ПЕРЕСЕЧЕНИЯ: Находит точку пересечения двух
		// линий.
		public static aLine.aPoint IntersectionPoint (aLine line1, aLine line2)
		{
			aLine.aPoint results = new aLine.aPoint() { success = false };
			double numerator1 = (line2.x2-line2.x1)*(line1.y1-line2.y1)-(line2.y2-line2.y1)*(line1.x1-line2.x1);
			double numerator2 = (line1.x2-line1.x1)*(line1.y1-line2.y1)-(line1.y2-line1.y1)*(line1.x1-line2.x1);
			double denominator = (line2.y2-line2.y1)*(line1.x2-line1.x1)-(line2.x2-line2.x1)*(line1.y2-line1.y1);
			if (numerator1 == 0 || numerator2 == 0 || denominator == 0) return results;
			double ua = numerator1/denominator;
			double ub = numerator2/denominator;
			if (!(((0.0 <= ua) && (ua <= 1.0) && ((0.0 <= ub) && (ub <= 1.0))))) 
				return results;
			results.x = line1.x1+ua*(line1.x2-line1.x1);
			results.y = line1.y1+ua*(line1.y2-line1.y1);
			results.success = true;
			return results;
		}
		
		// ПОЛУЧИТЬ МНОЖЕСТВО ТОЧЕК ПЕРЕСЕЧЕНИЯ:
		// Возвращает множество точек пересечения двух
		// множеств линий.
		public static SortedList GetIntrPointSet (ArrayList LineSet1, ArrayList LineSet2)
		{
			SortedList results = new SortedList();
			aLine.aPoint IntrPoint = new aLine.aPoint();
			foreach (aLine elem1 in LineSet1)
			{ foreach (aLine elem2 in LineSet2)
				{
					IntrPoint = IntersectionPoint(elem1, elem2);
					if (IntrPoint.success) results.Add(IntrPoint.x, IntrPoint.y);
				}}
			return results;
		}		
	}	
}