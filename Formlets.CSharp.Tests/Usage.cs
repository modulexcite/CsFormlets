﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Formlets.CSharp.Tests {
    public class Usage {
        [Fact]
        public void Run() {
            var input = Formlet.Input();
            var result = input.Run(new Dictionary<string, string> {{"input_0", "something"}});
            Assert.True(result.Value.IsSome());
            Assert.Equal("something", result.Value.Value);
            Console.WriteLine(result.ErrorForm);
            Assert.Equal("<input name=\"input_0\" value=\"something\" />", result.ErrorForm.ToString());
        }

        [Fact]
        public void Render() {
            var input = Formlet.Input("a value", new Dictionary<string, string> {{"size", "10"}});
            var html = input.Render();
            Console.WriteLine(html);
            Assert.Equal("<input name=\"input_0\" value=\"a value\" size=\"10\" />", html);
        }

        [Fact]
        public void Lift() {
            var input = Formlet.Input();
            var inputInt = input.Lift(int.Parse);
            var result = inputInt.Run(new Dictionary<string, string> {{"input_0", "15"}});
            Assert.True(result.Value.IsSome());
            Assert.Equal(15, result.Value.Value);
        }

        [Fact]
        public void PureApply() {
            var input = Formlet.Input("a value", new Dictionary<string, string> {{"size", "10"}});
            var inputInt = Formlet.Input().Lift(int.Parse);
            var formlet = Formlet.Yield(L.F((string a) => L.F((int b) => Tuple.Create(a,b))))
                .Ap(input)
                .Ap("Hello world!")
                .Ap(inputInt);
            var html = formlet.Render();
            Assert.Contains("<input name=\"input_0\" value=\"a value\" size=\"10\" />", html);
            Assert.Contains("Hello world!", html);
            Assert.Contains("<input name=\"input_1\" value=\"\" />", html);
            var result = formlet.Run(new Dictionary<string, string> {
                {"input_0", "bla"},
                {"input_1", "20"},
            });
            Assert.Equal("bla", result.Value.Value.Item1);
            Assert.Equal(20, result.Value.Value.Item2);
        }

        [Fact]
        public void Validation() {
            var inputInt = Formlet.Input()
                .Satisfies(s => Regex.IsMatch(s, "[0-9]+"), (s, n) => {
                    var msg = string.Format("'{0}'is not a valid number", s);
                    n.Add(new XText(msg));
                    return n;
                })
                .Lift(int.Parse);
            var result = inputInt.Run(new Dictionary<string, string> {
                {"input_0", "bla"}
            });
            Console.WriteLine(result.ErrorForm);
            Assert.Contains("<input name=\"input_0\" value=\"bla\" />'bla'is not a valid number", result.ErrorForm.ToString());
            Assert.True(result.Value.IsNone());
        }

        [Fact]
        public void Validation2() {
            var inputInt = Formlet.Input()
                .Satisfies(s => Regex.IsMatch(s, "[0-9]+"), 
                           s => string.Format("'{0}'is not a valid number", s))
                .Lift(int.Parse);
            var result = inputInt.Run(new Dictionary<string, string> {
                {"input_0", "bla"}
            });
            Console.WriteLine(result.ErrorForm);
            Assert.Contains("<input name=\"input_0\" value=\"bla\" />", result.ErrorForm.ToString());
            Assert.Contains("<span class=\"error\">'bla'is not a valid number</span>", result.ErrorForm.ToString());
            Assert.True(result.Value.IsNone());            
        }

    }
}