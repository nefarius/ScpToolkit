using CSScriptLibrary;
using System;

//Read in more details about all aspects of CS-Script hosting in applications here: http://www.csscript.net/help/Script_hosting_guideline_.html
//You can also find the example of all possible hosting scenarios (including debugging) in the <cs-script>\Samples\Hosting folder 
//In most of the cases CodeDOM approach is the most flexible and natural choice, though Mono (Emulator) has some advantages with 
//respect to memory management.

class Program
{
    static void Main()
    {
        HostApp.Test();
    }
}

public interface ICalc
{
    HostApp Host { get; set; }
    int Sum(int a, int b);
}

public class HostApp
{
    public static void Test()
    {
        var host = new HostApp();

        host.Log("CodeDOM Tests...");
        CodeDom.HelloTest_1();
        CodeDom.HelloTest_2();
        CodeDom.CalcTest_InterfaceInheritance(host);
        CodeDom.CalcTest_InterfaceAlignment(host);

        host.Log("Mono CompilerAsService Tests...");
        CompilerAsService.HelloTest_1();
        CompilerAsService.HelloTest_2();
        CompilerAsService.CalcTest_InterfaceInheritance(host);
        CompilerAsService.CalcTest_InterfaceAlignment(host);
    }

    class CodeDom
    {
        public static void HelloTest_1()
        {
            //the 'using System;' is not required but it demonstrates how to specify 'usings' in the method-only syntax
            var SayHello = CSScript.LoadDelegate<Action<string>>(
                                             @"using System;    
                                               void SayHello(string greeting)
                                               {
                                                   Console.WriteLine(greeting);
                                               }");

            SayHello("Hello World!");
        }

        public static void HelloTest_2()
        {
            var SayHello = CSScript.LoadMethod(@"static void SayHello(string greeting)
                                                 {
                                                     Console.WriteLine(greeting);
                                                 }")
                                     .GetStaticMethod();

            SayHello("Hello World!");
        }


        public static void CalcTest_InterfaceInheritance(HostApp host)
        {
            ICalc calc = (ICalc)CSScript.LoadCode(@"public class Script : ICalc
                                                    { 
                                                        public int Sum(int a, int b)
                                                        {
                                                            if(Host != null) 
                                                                Host.Log(""Sum is invoked"");
                                                            return a + b;
                                                        }
                                                    
                                                        public HostApp Host { get; set; }
                                                    }")
                                         .CreateObject("*");
            calc.Host = host;
            int result = calc.Sum(1, 2);
        }

        public static void CalcTest_InterfaceAlignment(HostApp host)
        {
            ICalc calc = CSScript.LoadCode(@"public class Script : ICalc
                                             { 
                                                 public int Sum(int a, int b)
                                                 {
                                                     if(Host != null) 
                                                         Host.Log(""Sum is invoked"");
                                                     return a + b;
                                                 }
                                             
                                                 public HostApp Host { get; set; }
                                             }")
                                 .CreateObject("*")
                                 .AlignToInterface<ICalc>();
            calc.Host = host;
            int result = calc.Sum(1, 2);
        }
    }

    class CompilerAsService
    {
        public static void HelloTest_1()
        {
            dynamic script = CSScript.Evaluator
                                     .LoadMethod(@"void SayHello(string greeting)
                                                   {
                                                       Console.WriteLine(greeting);
                                                   }");

            script.SayHello("Hello World!");
        }

        public static void HelloTest_2()
        {
            var SayHello = CSScript.Evaluator
                                   .LoadDelegate<Action<string>>(
                                               @"void SayHello(string greeting)
                                                 {
                                                     Console.WriteLine(greeting);
                                                 }");
            SayHello("Hello World!");
        }

        public static void CalcTest_InterfaceInheritance(HostApp host)
        {
            ICalc calc = (ICalc)CSScript.Evaluator
                                        .LoadCode(@"
                                                public class Script : ICalc
                                                { 
                                                    public int Sum(int a, int b)
                                                    {
                                                        if(Host != null) 
                                                            Host.Log(""Sum is invoked"");
                                                        return a + b;
                                                    }
                             
                                                    public HostApp Host { get; set; }
                                                }");
            calc.Host = host;
            int result = calc.Sum(1, 2);
        }

        public static void CalcTest_InterfaceAlignment(HostApp host)
        {
            ICalc calc = CSScript.Evaluator
                                 .LoadMethod<ICalc>(@"
                                                  public int Sum(int a, int b)
                                                  {
                                                      if(Host != null) 
                                                          Host.Log(""Sum is invoked"");
                                                      return a + b;
                                                  }

                                                  public HostApp Host { get; set; }");
            calc.Host = host;
            int result = calc.Sum(1, 2);
        }
    }

    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}
