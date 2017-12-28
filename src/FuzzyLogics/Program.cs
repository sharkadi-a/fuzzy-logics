 /*
 * Created by SharpDevelop.
 * User: Андрей
 * Date: 22.04.2010
 * Time: 19:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using FuzzyLogics;

namespace FuzzyLogicsSample
{
	
	class Program
	{
		public static void Main(string[] args)
		{
			LinguisticVariable A = new LinguisticVariable(new FuzzySet("A", 1, 0, 2, 4, 5, 4, 6, 0));
//			LinguisticVariable B = new LinguisticVariable(new FuzzySet("B", 2, 0, 3, 2, 4, 2, 5, 0));
			aLine.aPoint[] points = new aLine.aPoint[4];
			points[0] = new aLine.aPoint(2, 0);
			points[1] = new aLine.aPoint(3, 2);
			points[2] = new aLine.aPoint(4, 2);
			points[3] = new aLine.aPoint(5, 0);
			LinguisticVariable B = new LinguisticVariable(new FuzzySet("B", points));
			LinguisticVariable Result = A & B;
			Console.WriteLine(Result);
			Console.ReadKey();
		}
	}
}