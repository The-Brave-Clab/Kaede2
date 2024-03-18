using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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

		public string String(int end, bool noTag = false)
		{
			return textNode.String(end, noTag);
		}

		public void Dump(int indent)
		{
			Dump(textNode, indent);
		}

		private void Dump(TextNode node, int indent)
		{
			string text = string.Empty;
			for (int i = 0; i < indent; i++)
			{
				text += "-";
			}
			foreach (var n in node.Children)
			{
				Debug.LogError(string.Concat(text, "[", n.NodeType.ToString("G"), "]", n.Text));
				if (n.Children.Count != 0)
				{
					Dump(n, indent + 1);
				}
			}
		}

		private enum NodeType
		{
			None,
			RootNode,
			TextNode,
			BoldNode,
			ItalicNode,
			ColorNode,
			SizeNode
		}

		private class TextNode
		{
			public int Length => text.Length + children.Sum(c => c.Length);
			public NodeType NodeType => nodeType;
			public string Text => text;
			public IReadOnlyList<TextNode> Children => children.AsReadOnly();

			private string text;
			private string option;
			private TextNode parent;

			private readonly NodeType nodeType;

			private readonly string prefix;
			private readonly string suffix;
			private readonly List<TextNode> children;

			private static readonly Regex RegexColor = new("\\(([#0-9a-zA-Z]+)\\)");
			private static readonly Regex RegexSize = new("\\(([0-9]+)\\)");

			private TextNode(NodeType type = NodeType.TextNode)
			{
				text = string.Empty;
				option = string.Empty;
				parent = null;
				nodeType = type;
				switch (type)
				{
					case NodeType.BoldNode:
						prefix = "<b>";
						suffix = "</b>";
						break;
					case NodeType.ItalicNode:
						prefix = "<i>";
						suffix = "</i>";
						break;
					case NodeType.ColorNode:
						prefix = "<color=%option%>";
						suffix = "</color>";
						break;
					case NodeType.SizeNode:
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

			public string String(int end, bool noTag = false)
			{
				if (Length < end)
				{
					end = Length;
				}
				int num = 0;
				string result = string.Empty;
				if (text != string.Empty)
				{
					int num2 = Math.Min(text.Length, end);
					num += num2;
					result += text.Substring(0, num2);
				}
				foreach (TextNode textNode in children)
				{
					if (num + textNode.Length < end)
					{
						num += textNode.Length;
						result += textNode.String(textNode.Length, noTag);
					}
					else
					{
						int num3 = end - num;
						num += num3;
						result += textNode.String(num3, noTag);
					}
					if (end <= num)
					{
						break;
					}
				}
				if (noTag)
				{
					return result;
				}
				return prefix.Replace("%option%", option) + result + suffix;
			}

			public static TextNode Parse(string source)
			{
				TextNode textNode1 = new TextNode(NodeType.RootNode);
				TextNode textNode2 = textNode1;
				for (int i = 0; i < source.Length; i++)
				{
					if (source[i] == '}' && textNode2 != null && textNode2.nodeType != NodeType.RootNode && textNode2.nodeType != NodeType.TextNode)
					{
						textNode2.parent.children.Add(textNode2);
						if (textNode2.parent.nodeType == NodeType.RootNode)
						{
							if (textNode2.text != string.Empty)
							{
								TextNode node = new TextNode()
								{
									text = textNode2.text
								};
								textNode2.text = string.Empty;
								textNode2.children.Add(node);
							}
							textNode2 = new TextNode
							{
								parent = textNode2.parent
							};
						}
						else
						{
							if (textNode2.text != string.Empty)
							{
								TextNode node2 = new TextNode()
								{
									text = textNode2.text
								};
								textNode2.text = string.Empty;
								textNode2.children.Add(node2);
							}
							textNode2 = textNode2.parent;
						}
					}
					else
					{
						if (source[i] == '@')
						{
							TextNode textNode3 = null;
							int num = 2;
							char c = source[i + 1];
							switch (c)
							{
								case 'b':
									textNode3 = new TextNode(NodeType.BoldNode);
									break;
								case 'c':
								{
									textNode3 = new TextNode(NodeType.ColorNode);
									num = source.IndexOf("{", i + num, StringComparison.Ordinal) - i;
									if (0 < num)
									{
										string input2 = source.Substring(i + 2, num - 1);
										var match2 = RegexColor.Match(input2);
										if (match2.Success && match2.Groups.Count == 2)
										{
											textNode3.option = match2.Groups[1].Value;
										}
									}

									break;
								}
								case 'i':
									textNode3 = new TextNode(NodeType.ItalicNode);
									break;
								case 's':
								{
									textNode3 = new TextNode(NodeType.SizeNode);
									num = source.IndexOf("{", i + num, StringComparison.Ordinal) - i;
									if (0 < num)
									{
										string input = source.Substring(i + 2, num - 1);
										var match = RegexSize.Match(input);
										if (match.Success && match.Groups.Count == 2)
										{
											textNode3.option = match.Groups[1].Value;
										}
									}

									break;
								}
							}

							if (textNode3 != null && 0 < num && source[i + num] == '{')
							{
								if (textNode2.nodeType == NodeType.RootNode)
								{
									textNode3.parent = textNode2;
									textNode2 = textNode3;
								}
								else if (textNode2.nodeType == NodeType.TextNode)
								{
									textNode2.parent.children.Add(textNode2);
									textNode3.parent = textNode2.parent;
									textNode2 = textNode3;
								}
								else
								{
									textNode3.parent = textNode2;
									if (textNode2.text != string.Empty)
									{
										TextNode node3 = new TextNode()
										{
											text = textNode2.text
										};
										textNode2.text = string.Empty;
										textNode2.children.Add(node3);
									}
									textNode2 = textNode3;
								}
								i += num;
								goto IL_335;
							}
						}
						if (textNode2 == textNode1)
						{
							textNode2 = new TextNode
							{
								parent = textNode1
							};
						}
						if (i < source.Length)
						{
							TextNode textNode4 = textNode2;
							textNode4.text += source[i];
						}
					}
					IL_335:;
				}
				while (textNode2 != textNode1)
				{
					textNode2.parent.children.Add(textNode2);
					textNode2 = textNode2.parent;
				}
				return textNode1;
			}
		}
	}
}
