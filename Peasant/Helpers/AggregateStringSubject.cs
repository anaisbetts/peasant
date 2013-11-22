using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Peasant.Helpers
{
    public class AggregateStringSubject : ISubject<string, string>
    {
        readonly StringBuilder buffer = new StringBuilder();
        readonly Subject<string> inner = new Subject<string>();

        public AggregateStringSubject()
        {
            inner.Subscribe(x => buffer.AppendLine(x));
        }

        public void OnCompleted() { inner.OnCompleted(); }
        public void OnError(Exception error) { inner.OnError(error); }
        public void OnNext(string value) { inner.OnNext(value); }

        public string Current {
            get { return buffer.ToString(); }
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return inner.Subscribe(observer);
        }
    }
}
