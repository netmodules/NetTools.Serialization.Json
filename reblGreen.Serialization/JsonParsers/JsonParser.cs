using System;
namespace reblGreen.Serialization
{
    public class JsonParser
    {
        enum Token
        {
            None,           // Used to denote no Lookahead available
            Curly_Open,
            Curly_Close,
            Squared_Open,
            Squared_Close,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null
        }

        public JsonParser()
        {
        }
    }
}
