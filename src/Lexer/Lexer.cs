using static System.Console;

using System;
using System.Collections.Generic;

using static Lune.Lexer.TokenType;
using static Lune.ErrorReporting;

namespace Lune.Lexer
{
    /// <summary>
    /// Lexical analyzer for Rune.
    /// </summary>
    public class Lexer
    {
        private int start = 0, current = 0;
        private int line = 1;

        private readonly string source;
        private List<Token> tokens = new List<Token>();

        // Initializer
        public Lexer(string source)
        {
            this.source = source;
        }

        private bool isAtEnd() => current >= source.Length;

        private char advance()
        {
            Console.WriteLine($"start = {start}, current = {current}");

            if (!isAtEnd())
                return source[current++];
            return '\0';
        }

        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private char peekNext(int ahead = 1)
        {
            if (current + ahead >= source.Length)
                return '\0';
            return source[current + ahead];
        }

        private bool matches(char expected)
        {
            if (isAtEnd())
                return false;

            if (source[current] != expected)
                return false;

            current++;
            return true;
        }

        private void addToken(TokenType type, object? literal = null)
        {
            Console.WriteLine($"start = {start}, current = {current}");

            var text = source.Substring(start, current - start);
            tokens.Add(new Token(type, "", literal, line));
        }

        /// <summary>
        /// Scan through until we reach the end of the string, and emit tokens
        /// </summary>
        /// <returns>List of all produced tokens</returns>
        public List<Token> Scan()
        {
            while (!isAtEnd())
            {
                // We are at the beginning of the next lexeme
                start = current;
                this.scanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void scanNumber()
        {
            while (Char.IsDigit(peek()))
                advance();

            if (peek() == '.' && Char.IsDigit(peekNext()))
            {
                advance();

                while (Char.IsDigit(peek()))
                    advance();
            }

            var value = source.Substring(start, current - start);
            addToken(NumberLit, Double.Parse(value));
        }

        private void scanString()
        {
            while (peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n')
                    line++;
            }

            if (isAtEnd())
            {
                Error(line, "Unterminated string literal");
                return;
            }

            // Closing quote
            advance();

            // Remove the surrounding quotes
            string value = source.Substring(start + 1, current - start - 1);
            addToken(StringLit, value);
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(': addToken(LeftParen); break;
                case ')': addToken(RightParen); break;
                case '{': addToken(LeftBrace); break;
                case '}': addToken(RightBrace); break;
                case ',': addToken(Comma); break;
                case '.': addToken(Dot); break;
                case '+': addToken(Plus); break;
                case '-': addToken(Minus); break;
                case '*': addToken(Star); break;
                case '/': addToken(Slash); break;
                case '!': addToken(matches('=') ? BangEqual : Bang); break;
                case '=': addToken(matches('=') ? EqualEqual : Equal); break;
                case '<': addToken(matches('=') ? LessEqual : Less); break;
                case '>': addToken(matches('=') ? GreaterEqual : Greater); break;
                case '"': scanString(); break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;
                case '\n':
                    line++;
                    break;
                default:
                    if (Char.IsDigit(c))
                    {
                        scanNumber();
                    }
                    else
                    {
                        Error(line, $"Unexpected character '{c}'");
                    }
                    break;
            }
        }
    }
}