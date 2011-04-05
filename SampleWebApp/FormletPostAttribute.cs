﻿using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Linq;
using Formlets.CSharp;

namespace SampleWebApp {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FormletPostAttribute : ActionFilterAttribute {
        private readonly string formletMethodName;

        public FormletPostAttribute(string formletMethodName) {
            this.formletMethodName = formletMethodName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var method = filterContext.Controller.GetType().GetMethod(formletMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (method == null)
                throw new Exception(string.Format("Formlet method '{0}' not found in controller '{1}'", formletMethodName, filterContext.Controller));
            dynamic formlet = method.Invoke(filterContext.Controller, null);
            dynamic result = formlet.RunPost(filterContext.HttpContext.Request);
            if (result.Value == null) {
                IEnumerable<XNode> errorNodes = result.ErrorForm;
                string errorForm = errorNodes.Render();
                filterContext.Result = new ViewResult {
                    ViewData = new ViewDataDictionary(errorForm)
                };
            } else {
                filterContext.ActionParameters[formletMethodName] = result.Value.Value;
            }
        }
    }
}