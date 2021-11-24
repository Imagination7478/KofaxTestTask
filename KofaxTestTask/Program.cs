using System;
using System.Collections.Generic;
using System.Linq;

namespace KofaxTestTask
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Write("Input equation: ");

            string equation = "";
            equation = Console.ReadLine();
            equation = equation.Replace(" ", ""); // Удаляем все пробелы из выражения.

            List<char> finalEquation = new List<char>();
            bool prevWasSymbol = false;
            var set = new HashSet<char>(); // сет для хранения неизвестных в выражениии

            for (int i = 0; i < equation.Count(); i++)
            {
                finalEquation.Add(equation[i]);
                if (isItSymbol(equation[i]))
                {
                    try
                    {
                        if (equation[i] == '(' || equation[i] == ')') { prevWasSymbol = false; continue; }
                        if (prevWasSymbol) { throw new Exception(); }
                        prevWasSymbol = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message); // Исключает воозможность написать 2 раза один знак
                        Environment.Exit(1);
                    }
                }
                else if (Char.IsDigit(equation[i]))
                {
                    prevWasSymbol = false;
                }
                else
                {
                    set.Add(equation[i]);
                    prevWasSymbol = false;
                }

            }

            // Цикл для замены неизвестных в выражении на числа
            for (int i = 0, count = set.Count(); i < count; i++)
            {
                Console.Write($"Input {set.First()}: ");

                string tmp = Console.ReadLine();
                char[] tmpCharArr = tmp.ToCharArray();

                // находим все вхождения неизвестных и заменяем их значениями
                int indexOfUnknown = finalEquation.IndexOf(set.First());
                while (indexOfUnknown != -1)
                {
                    finalEquation.Remove(set.First());
                    for (int k = 0; k < tmpCharArr.Count(); k++)
                        finalEquation.Insert(indexOfUnknown + k, tmpCharArr[k]);
                    indexOfUnknown = finalEquation.IndexOf(set.First());
                }
                set.Remove(set.First());
            }
            string newEquation = "";

            for (int i = 0; i < finalEquation.Count(); i++)
            {
                newEquation = $"{newEquation}{finalEquation[i]}";
            }


            equation = postfix(finalEquation);
            //Console.WriteLine($"Postfix write: {equation}"); 
            char[] arr = equation.ToCharArray();
            Array.Reverse(arr); // инвертируем строку, т.к. последнее действие - корень

            AstNode AST = createAst(new string(arr));

            double answer = countAst(AST);
            Console.WriteLine($"Answer: {answer}");
        }

        static int Priority(char symbol)
        {
            switch (symbol) //Получаем на входе символ, и возвращаем его приоритет.
            {
                case '*': return 4;
                case '/': return 4;
                case '+': return 3;
                case '-': return 3;
                case ')': return 2;
                case '(': return 1;
                default: return 0;
            }
        }
        static AstNode createAst(string ast)
        {
            AstNode node;
            AstNode tmp;
            int i = 0;
            node = new AstNode(ast[i++].ToString()); // задаем корень
            while (true)
            {
                while (node.getRChild() == null) // если нет правого ребенка
                {
                    if (ast[i] == ' ') { i++; continue; }
                    tmp = setChild(node, ast, ref i, 1); // задаем ребенка с флагом 1 (правый)
                    node = tmp;
                    if (node.getType() == 1) { node = node.getParent(); i++; break; } // если тип ноды - число, то возвращаемся к родителю
                    i++;
                }
                while (node.getLChild() == null) // если нет левого
                {
                    if (node.getRChild() == null) break; // если нет и левого, и правого, идем сперва в правого
                    if (ast[i] == ' ') { i++; continue; } // пропускаем пробелы
                    tmp = setChild(node, ast, ref i, 0); // флаг 0 - левый
                    node = tmp;
                    if (node.getType() == 1) { node = node.getParent(); i++; break; } 
                    i++;
                }
                // Проверяем, если у ноды есть оба ребенка, и родитель, то возвращаемся к родителю
                // как только находим корень, выходим из цикла
                if (node.getParent() != null && node.getLChild() != null && node.getRChild() != null) node = node.getParent();
                if (node.getParent() == null) break;
            }
            return node;
        }
        static double countAst(AstNode AST)
        {
            // переменные, чтобы обозначить ноду как проверенную и посчитанную
            bool lChildChecked = false, rChildChecked = false;

            double resultR = 0;
            double resultL = 0;

            while (!lChildChecked || !rChildChecked) // пока хотя бы один из них false
            {
                // если левый или правый ребенок не число, то
                // рекурсивно заходим в них
                // и считаем результат в этой ветке, помечаем ветку просмотренной
                if (AST.getRChild().getType() != 1 && !rChildChecked) 
                {
                    resultR = countAst(AST.getRChild()); 
                    rChildChecked = true;
                }
                else if (AST.getLChild().getType() != 1 && !lChildChecked)
                {
                    resultL = countAst(AST.getLChild());
                    lChildChecked = true;
                }
                // если дети - числа, то считаем их операцией нашей ноды
                else if (AST.getLChild().getType() == 1 && AST.getRChild().getType() == 1)
                {
                    try
                    {
                        switch (AST.getType())
                        {
                            case 11: // +
                                return Int32.Parse(AST.getLChild().getText()) + Int32.Parse(AST.getRChild().getText());
                            case 12: // -
                                return Int32.Parse(AST.getLChild().getText()) - Int32.Parse(AST.getRChild().getText());
                            case 13: // *
                                return Int32.Parse(AST.getLChild().getText()) * Int32.Parse(AST.getRChild().getText());
                            case 14: // /
                                if (Int32.Parse(AST.getRChild().getText()) == 0) throw new Exception();
                                return Int32.Parse(AST.getLChild().getText()) / Int32.Parse(AST.getRChild().getText());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // если у нас посчитана правая ветка, а левая просто число, то
                // задаем, результатом левой ветки это число
                if (rChildChecked && AST.getLChild().getType() == 1)
                {
                    resultL = Int32.Parse(AST.getLChild().getText());
                    lChildChecked = true;
                }

                // аналогично, но с правой веткой
                if (lChildChecked && AST.getRChild().getType() == 1)
                {
                    resultR = Int32.Parse(AST.getRChild().getText());
                    rChildChecked = true;
                }
            }


            // подсчитываем результат левой и правой ветки
            // и возвращаем результат прошлой ноде
            try
            {
                switch (AST.getType())
                {
                    case 11: // +
                        return resultL + resultR;
                    case 12: // -
                        return resultL - resultR;
                    case 13: // *
                        return resultL * resultR;
                    case 14: // /
                        if (resultR == 0) throw new Exception();
                        return resultL / resultR;      
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
            return 0;
        }
        static AstNode setChild(AstNode node, string ast, ref int i, int flag) // flag = 0 left, 1 - right
        {
            AstNode tmp = null;

            // если в строке мы нашли число, то оно может быть многозначным
            // для этого передаем параметр i как ссылку
            // и считываем строку до момента пробела, либо до конца строки
            if (Char.IsDigit(ast[i])) 
            {
                string num = "";

                for (; i < ast.Count() && ast[i] != ' '; i++)
                    num += ast[i];

                char[] numArr = num.ToCharArray();
                Array.Reverse(numArr); // инвертируем строку, чтобы записать число корректно

                tmp = new AstNode(new string(numArr), node);
            }
            else
                tmp = new AstNode(ast[i].ToString(), node);

            if (flag == 1) node.setRChild(tmp); else if (flag == 0) node.setLChild(tmp);
            return tmp;
                
            
        }
        static string postfix(List<char> finalEquation)
        {
            var MyStack = new Stack<char>(); //Объявлен стек для операций
            string x = null;
            int symbolPriority;

            for (int i = 0; i < finalEquation.Count(); i++)
            {
                if (Char.IsDigit(finalEquation[i])) // Если число
                {
                    if (i > 0 && !Char.IsDigit(finalEquation[i - 1]))
                        x += " ";
                    x += Convert.ToString(finalEquation[i]); // то вписываем в строку, разделяем пробелами
                }
                else
                {
                    symbolPriority = Priority(finalEquation[i]);
                    if (MyStack.Count == 0) //Если стек пустой
                    {
                        MyStack.Push(finalEquation[i]); //Закидываем знак в стек
                    }
                    else //
                    {
                        if (symbolPriority != Priority(MyStack.Peek())) //Если символы по приоритету не равны
                        {
                            if (symbolPriority > Priority(MyStack.Peek())) //Если приоритет входящего символа больше имеющегося
                            {
                                if (symbolPriority == 2 && MyStack.Peek() == '(') //Если символ является закрывающей скобкой, а наверху - открывающая
                                    MyStack.Pop();
                                else
                                    MyStack.Push(finalEquation[i]);
                            }
                            else if (symbolPriority < Priority(MyStack.Peek()))
                            {

                                if (symbolPriority == 2) //Если символ является закрывающей скобкой
                                {
                                    if (MyStack.Peek() == '(') //Если на вершине стека находится открывающая скобка
                                    {
                                        MyStack.Pop();
                                    }
                                    else  //Если не равен открывающей скобке
                                    {
                                        while (MyStack.Peek() != '(') //Пока на вершине не окажется открывающая скобка
                                        {
                                            x = $"{x}{MyStack.Pop()}"; //Выталкиваем в строку символы
                                        }
                                        MyStack.Pop();
                                    }
                                }
                                else
                                {
                                    if (symbolPriority == 1) //Если равен открывающей скобке
                                    {
                                        MyStack.Push(finalEquation[i]);
                                    }
                                    else
                                    {
                                        x += MyStack.Pop();
                                        MyStack.Push(finalEquation[i]);
                                    }
                                }

                            }
                        }
                        else if (symbolPriority == Priority(MyStack.Peek()))//Если символы по приоритету равны
                        {
                            x = $"{x}{MyStack.Pop()}"; //Из вершины стека выкидываем символ в строку
                            MyStack.Push(finalEquation[i]); //Кладем в стек новый символ
                        }
                    }
                }
            }
            while (MyStack.Count > 0)//Выкидываем остаток символов из стека
            {
                x = $"{x}{MyStack.Pop()}";
            }
            return x;
        }
        static bool isItSymbol(char symb)
        {
            if (symb == '*' ||
                symb == '/' ||
                symb == '+' ||
                symb == '-' ||
                symb == '(' ||
                symb == ')')
            {
                return true;
            }
            return false;
        }
    }
}
