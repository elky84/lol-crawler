using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebUtil.Util
{
    public static class ClassUtil
    {
        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            var body = ((MemberExpression)memberAccess.Body);
            return body.Member.Name;
        }

        public static string GetMemberNameWithDeclaringType<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            var body = ((MemberExpression)memberAccess.Body);
            return $"{body.Member.DeclaringType.Name}.{body.Member.Name}";
        }
    }
}
