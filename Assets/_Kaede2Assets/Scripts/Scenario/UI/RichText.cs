using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using YamlDotNet.Core;

namespace Kaede2.Scenario.UI
{
    public class RichText
    {
        public static bool DumpText;

        private readonly TextNode textNode;

        public string MacroText { get; }

        public int Length => textNode.Length;

        public RichText(string macroText)
        {
            MacroText = macroText;
            textNode = TextNode.Parse(macroText);
            if (DumpText)
            {
                Debug.LogError(macroText);
                Dump(0);
            }
        }

        public override string ToString()
        {
            return String();
        }

        public string String(int end = -1, bool noTag = false)
        {
            return Substring(0, end, noTag);
        }

        public string Substring(int index, int length, bool noTag = false)
        {
            return textNode.Substring(index, length, !noTag);
        }

        public void Dump(int indent)
        {
            StringBuilder logText = new();
            DumpLog(textNode, 0, indent, logText);
            Debug.Log(logText.ToString());
        }

        private static void DumpLog(TextNode node, int indentLevel, int indentSize, StringBuilder logText)
        {
            for (int i = 0; i < indentLevel * indentSize; i++)
            {
                logText.Append("-");
            }
            logText.Append("[");
            logText.Append(node.NodeType.ToString("G")[0]);
            if (node.Option != string.Empty)
                logText.Append($"({node.Option})");
            logText.Append("]");
            if (node.Text != string.Empty)
                logText.Append($"\"{node.Text.Replace("\n", "\\n")}\"");
            logText.Append("\n");
            foreach (var n in node.Children)
            {
                DumpLog(n, indentLevel + 1, indentSize, logText);
            }
        }

        private enum NodeType
        {
            Root,
            Text,
            Bold,
            Italic,
            Color,
            Size,
        }

        private class TextNode
        {
            public int Length => text.Length + children.Sum(c => c.Length);
            public NodeType NodeType => nodeType;
            public string Text => text;
            public string Option => option;
            public IReadOnlyList<TextNode> Children => children.AsReadOnly();

            private string text;
            private string option;

            private readonly NodeType nodeType;

            private readonly string prefix;
            private readonly string suffix;
            private readonly List<TextNode> children;

            private static readonly Regex RegexColor = new(@"\((#[0-9a-fA-F]{6})\)");
            private static readonly Regex RegexSize = new(@"\(([0-9]+)\)");

            private TextNode(NodeType type = NodeType.Text)
            {
                text = string.Empty;
                option = string.Empty;
                nodeType = type;
                switch (type)
                {
                    case NodeType.Bold:
                        prefix = "<b>";
                        suffix = "</b>";
                        break;
                    case NodeType.Italic:
                        prefix = "<i>";
                        suffix = "</i>";
                        break;
                    case NodeType.Color:
                        prefix = "<color=%option%>";
                        suffix = "</color>";
                        break;
                    case NodeType.Size:
                        prefix = "<size=%option%>";
                        suffix = "</size>";
                        break;
                    default:
                        prefix = string.Empty;
                        suffix = string.Empty;
                        break;
                }
                children = new();
            }

            // the rich text is made with text and following special tags
            // @b{<text>} : bold
            // @i{<text>} : italic
            // @c(<option>){<text>} : color
            // @s(<option>){<text>} : size
            // the whole string will form a tree structure
            // for example:
            // This is a @b{rich @c(#ff0000){text} with @i{multiple}} tags
            // will be parsed into:
            // [Root]
            // - [Text] This is a
            // - [Bold]
            //   - [Text] rich
            //   - [Color<#ff0000>]
            //     - [Text] text
            //   - [Text] with
            //   - [Italic]
            //     - [Text] multiple
            // - [Text] tags
            // note that non-Text type nodes should not have text
            public static TextNode Parse(string text)
            {
                int position = 0;
                TextNode rootNode = new TextNode(NodeType.Root);
                ParseRecursive(text, rootNode, ref position);
                return rootNode;
            }

            public string Substring(int startIndex, int length, bool withTag)
            {
                int fullLength = Length;
                startIndex = Math.Clamp(startIndex, 0, fullLength);
                length = length < 0 ? fullLength - startIndex : Math.Clamp(length, 0, fullLength - startIndex);

                int position = 0;
                StringBuilder result = new();
                SubstringRecursive(ref startIndex, ref length, withTag, ref position, result);
                return result.ToString();
            }

            private void SubstringRecursive(ref int index, ref int length, bool withTag, ref int position, StringBuilder result)
            {
                int fullLength = Length;

                // we need to determine which node the index is in first
                if (position + fullLength <= index)
                {
                    position += fullLength;
                    return;
                }

                StringBuilder selfResult = new();

                // first, add text from this node
                var startIndex = index - position;
                if (startIndex <= text.Length)
                {
                    int selfTextLength = Math.Min(text.Length - startIndex, length);
                    selfResult.Append(text.Substring(startIndex, selfTextLength));
                    length -= selfTextLength;
                    index += selfTextLength;
                    position = index;
                }

                // then, add text from children
                foreach (var childNode in Children)
                {
                    if (length == 0)
                        break;

                    childNode.SubstringRecursive(ref index, ref length, withTag, ref position, selfResult);
                }

                // add prefix and suffix
                if (withTag)
                    result.Append(prefix.Replace("%option%", option));
                result.Append(selfResult);
                if (withTag)
                    result.Append(suffix);
            }

            private static void ParseRecursive(string text, TextNode parentNode, ref int position)
            {
                while (position < text.Length)
                {
                    // find next '@'
                    var nextTagPosition = text.IndexOf('@', position);
                    // find next '}'
                    var nextClosePosition = text.IndexOf('}', position);

                    // if not found, just create a text node as child
                    if (nextTagPosition == -1 && nextClosePosition == -1)
                    {
                        parentNode.children.Add(new TextNode { text = text[position..] });
                        position = text.Length;
                        break;
                    }

                    // if '}' comes first, close current tag and return to parent
                    // this operation should be ignored if the parent is root
                    if (nextClosePosition != -1 &&
                        (nextTagPosition == -1 || nextClosePosition < nextTagPosition) &&
                        parentNode.nodeType != NodeType.Root)
                    {
                        if (nextClosePosition != position)
                            parentNode.children.Add(new TextNode { text = text.Substring(position, nextClosePosition - position) });
                        position = nextClosePosition + 1;
                        return;
                    }

                    if (nextTagPosition == -1)
                    {
                        // if nextTagPosition is -1, it means there is no more tag
                        // create a text node with the rest of the text and return
                        parentNode.children.Add(new TextNode { text = text[position..] });
                        position = text.Length;
                        return;
                    }

                    // if found and nextPosition is not position (indicating there is some text before the tag)
                    // create a text node with the text before the tag
                    if (nextTagPosition != position)
                    {
                        parentNode.children.Add(new TextNode { text = text.Substring(position, nextTagPosition - position) });

                        position = nextTagPosition;// now text[position] is '@'
                    }

                    // get note type and option
                    if (GetTypeAndOption(text, ref position, out var childType, out var childOption))
                    {
                        var newNode = new TextNode(childType) { option = childOption };
                        parentNode.children.Add(newNode);
                        ParseRecursive(text, newNode, ref position);
                    }
                    else
                    {
                        // invalid tag
                        // at this point, text[position] must be '@', so we need to create a text with only '@' and skip to next
                        parentNode.children.Add(new TextNode { text = "@" });
                        ++position;
                    }
                }
            }

            // when entering this function, text[position] is '@'
            // when exiting this function, text[position - 1] is '{'
            // or if the input is invalid, position will not change and function returns false
            // option will be set to the option string, without the parenthesis
            // for example, for rich text:
            // This is @s(12){small} text
            // --------------------------
            // 01234567890123456789012345
            // 0         1         2
            // when entering, position is 8
            // when exiting, position is 14, option is "12"
            private static bool GetTypeAndOption(string text, ref int position, out NodeType type, out string option)
            {
                option = string.Empty;

                void InvalidateInput(int correctPosition, out int position, out NodeType type, out string option)
                {
                    position = correctPosition;
                    type = NodeType.Text;
                    option = string.Empty;
                }

                if (text[position] != '@')
                {
                    // invalid input, just treat the whole thing as text node
                    InvalidateInput(position, out position, out type, out option);
                    return false;
                }

                ++position;// now text[position] is the specifier
                char specifier = text[position];
                ++position;// skip the specifier, text[position] should be '{' or '(' depending on the specifier

                type = specifier switch
                {
                    'b' => NodeType.Bold,
                    'i' => NodeType.Italic,
                    'c' => NodeType.Color,
                    's' => NodeType.Size,
                    _ => NodeType.Text
                };

                // invalid specifier, treat the whole thing as text node
                if (type == NodeType.Text)
                {
                    // position need to be reset to the '@'
                    InvalidateInput(position - 2, out position, out type, out option);
                    return false;
                }

                if (type is NodeType.Bold or NodeType.Italic)
                {
                    // these two won't have option
                    if (text[position] != '{')
                    {
                        // invalid input, treat the whole thing as text node
                        InvalidateInput(position - 2, out position, out type, out option);
                        return false;
                    }
                    ++position; // now text[position] is the start of the text
                    return true;
                }

                // now text[position] is '('
                // try to match option with regex
                Regex regex = type == NodeType.Color ? RegexColor : RegexSize;
                var match = regex.Match(text, position);
                if (!match.Success || match.Groups.Count != 2 || match.Index != position)
                {
                    InvalidateInput(position - 2, out position, out type, out option);
                    return false;
                }

                option = match.Groups[1].Value;
                var beginOfBlock = match.Index + match.Length; // text[beginOfBlock] should be '{'
                if (text[beginOfBlock] != '{')
                {
                    // back off position to '@'
                    InvalidateInput(position - 2 , out position, out type, out option);
                    return false;
                }
                position = beginOfBlock + 1; // now text[position] is the start of the text
                return true;
            }
        }
    }
}
