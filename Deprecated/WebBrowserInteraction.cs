        public string ColorToHtml(Color c)
        {
            return string.Format("#{0}{1}{2}", c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
        }

        public Color HtmlToColor(string s)
        {
            return ColorTranslator.FromHtml(s);
        }

        public static string GetHtml(CDShape Shape)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.AppendLine("<style>");
            sb.AppendLine("td.choice {");
            sb.AppendLine("   padding:9pt;");
            sb.AppendLine("   align:center;");
            sb.AppendLine("   vertical-align:middle;");
            sb.AppendLine("   font-family:Helvetica;");
            sb.AppendLine("   font-size:9pt;cursor:pointer;");
            sb.AppendLine("}");
            sb.AppendLine("</style>");
            sb.AppendLine("<script>");
            sb.AppendLine("function disp(elemId) {");
            sb.AppendLine("  document.getElementById(elemId).style.display='block';");
            sb.AppendLine("}");
            sb.AppendLine("function hide(elemId) {");
            sb.AppendLine("  document.getElementById(elemId).style.display='none';");
            sb.AppendLine("}");
            sb.AppendLine("function notify(key,val) {");
            sb.AppendLine("  window.external.OnValueChange(key,val);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            sb.AppendLine("</head><body>");
            int index = 1;

            foreach (CSParameterDef p in Shape.ShapeDefinition.parameters)
            {
                if (p.ParameterType == CSParameterType.String)
                {
                    sb.AppendFormat("<p><b>{0}</b><br>", p.Name);
                    sb.AppendFormat("<input type=\"text\" id=\"e{2}\" name=\"{1}\" value=\"{0}\" "
                        + "onkeyup='notify(\"{1}\",document.getElementById(\"e{2}\").value)'>",
                        Shape.GetString(p.Name),
                        p.Name,
                        index);
                }
                else if (p.ParameterType == CSParameterType.Color)
                {
                    sb.AppendFormat("<p><b>{0}</b><br>", p.Name);
                    InsertColorPicker(sb, "e" + index, p.Name);
                }
                else if (p.ParameterType == CSParameterType.FontSize)
                {
                    sb.AppendFormat("<p><b>{0}</b><br>", p.Name);
                    InsertValuePicker(sb, "e" + index, p.Name, "9pt", "10pt", "11pt", "12pt", "",
                        "14pt", "16pt", "18pt", "20pt");
                }
                else if (p.ParameterType == CSParameterType.LineWidth)
                {
                    sb.AppendFormat("<p><b>{0}</b><br>", p.Name);
                    InsertValuePicker(sb, "e" + index, p.Name, "1.0", "1.5", "2.0", "3.0", "4.0", "6.0");
                }
                else if (p.ParameterType == CSParameterType.TextAlign)
                {
                    sb.AppendFormat("<p><b>{0}</b><br>", p.Name);
                    InsertTextAlignSelector(sb, p);
                }

                index++;
            }

            return sb.ToString();
        }

        private static void InsertTextAlignSelector(StringBuilder sb, CSParameterDef p)
        {
            sb.AppendLine("<table cellspacing=0>");
            sb.AppendLine("<tr>");
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopLeftOutside\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopTopLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopTop\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopTopRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopRightOutside\")'>&#x25A0;</td>", p.Name);
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"LeftTopLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-left:1px solid black;border-top:1px solid black;' onclick='notify(\"{0}\",\"TopLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-top:1px solid black;' onclick='notify(\"{0}\",\"Top\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-right:1px solid black;border-top:1px solid black;' onclick='notify(\"{0}\",\"TopRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"TopRightRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"LeftLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-left:1px solid black;' onclick='notify(\"{0}\",\"Left\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"Center\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-right:1px solid black;' onclick='notify(\"{0}\",\"Right\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"RightRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"LeftBottomLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-left:1px solid black;border-bottom:1px solid black;' onclick='notify(\"{0}\",\"BottomLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-bottom:1px solid black;' onclick='notify(\"{0}\",\"Bottom\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;border-right:1px solid black;border-bottom:1px solid black;' onclick='notify(\"{0}\",\"BottomRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomRightRight\")'>&#x25A0;</td>", p.Name);
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomLeftOutside\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomBottomLeft\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomBottom\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomRightBottom\")'>&#x25A0;</td>", p.Name);
            sb.AppendFormat("  <td style='cursor:pointer;align:center;vertical-align:middle;padding:4pt;' onclick='notify(\"{0}\",\"BottomRightOutside\")'>&#x25A0;</td>", p.Name);
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
        }

        public static int[,] baseColors = 
        {
            {255,0,0},{255,128,0},{255,255,0},{128,255,0},
            {0,255,0},{0,255,128},{0,255,255},{0,128,255},
            {0,0,255},{128,0,255},{255,0,255},{255,0,128}
        };

        public static void InsertValuePicker(StringBuilder sb, string elId, string elemName, params string[] values)
        {
            sb.Append("<table>");
            sb.Append("<tr>");

            foreach (string s in values)
            {
                if (string.IsNullOrEmpty(s))
                {
                    sb.Append("<tr>");
                }
                else
                {
                    sb.Append("<td width=30 height=20 ");
                    sb.AppendFormat(" class=\"choice\"");
                    sb.AppendFormat(" onclick='notify(\"{0}\",\"{1}\")'", elemName, s);
                    sb.AppendFormat(">{0}</td>", s);
                }
            }

            sb.AppendLine("</table>");
        }

        public static void InsertColorPicker(StringBuilder sb, string elId, string elemName)
        {
            int tr, tg, tb;

            sb.Append("<table>");
            sb.Append("<tr>");
            for (int i = 0; i < 12; i++)
            {
                sb.AppendFormat("<td width=15 height=15 onclick='");

                for (int m = 0; m < i; m++)
                {
                    sb.AppendFormat("hide(\"{0}_blk{1}\");", elId, m);
                }
                sb.AppendFormat("disp(\"{0}_blk{1}\");", elId, i);
                for (int m = i + 1; m < 12; m++)
                {
                    sb.AppendFormat("hide(\"{0}_blk{1}\");", elId, m);
                }
                sb.AppendFormat("'");
                sb.AppendFormat(" style='background:#{0}{1}{2}'",
                    baseColors[i, 0].ToString("X2"), baseColors[i, 1].ToString("X2"), baseColors[i, 2].ToString("X2"));
                sb.AppendFormat("></td>");
            }
            sb.Append("</tr>");
            sb.Append("</table>");

            for (int k = 0; k < 12; k++)
            {
                sb.AppendFormat("<div id='{0}_blk{1}' style='display:none'>", elId, k);
                sb.Append("<table>");

                tr = baseColors[k, 0];
                tg = baseColors[k, 1];
                tb = baseColors[k, 2];
                for (int i = 0; i < 8; i++)
                {
                    sb.Append("<tr>");
                    for (int j = 0; j < 16; j++)
                    {
                        int hr, hg, hb;
                        hr = Pwd(i, 8, Pwd(j, 16, 255, tr), 0);
                        hg = Pwd(i, 8, Pwd(j, 16, 255, tg), 0);
                        hb = Pwd(i, 8, Pwd(j, 16, 255, tb), 0);
                        string clr = string.Format("#{0}{1}{2}", hr.ToString("X2"),
                            hg.ToString("X2"),
                            hb.ToString("X2"));
                        sb.AppendFormat("<td width=10 height=10 onclick='notify(\"{0}\",\"{1}\")' ", elemName, clr);
                        sb.AppendFormat("style='border-color:black;background:{0}'", clr);
                        sb.Append(">");
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</table>");
                sb.AppendLine("</div>");
            }
        }

        public static int Pwd(int i, int mx, int val0, int val1)
        {
            return val0 + (int)(((float)i / mx) * (val1 - val0));
        }

		// before class with method OnValueChange
		    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
            [System.Runtime.InteropServices.ComVisibleAttribute(true)]

        public void OnValueChange(string tag, string value)
        {
            if (SelectedShape != null)
            {
                CSParameterDef p = SelectedShape.ShapeDefinition.GetParam(tag);
                if (p.ParameterType == CSParameterType.String)
                {
                    SelectedShape.SetString(tag, value);
                    scheduleInvalidateDiagram();
                }
                else if (p.ParameterType == CSParameterType.Color)
                {
                    SelectedShape.SetColor(tag, HtmlToColor(value));
                    caseDiagramView1.Invalidate();
                }
                else if (p.ParameterType == CSParameterType.FontSize)
                {
                    if (value.EndsWith("pt"))
                    {
                        SelectedShape.SetFloat(tag, value.Substring(0, value.Length - 2));
                        caseDiagramView1.Invalidate();
                    }
                }
                else if (p.ParameterType == CSParameterType.LineWidth)
                {
                    SelectedShape.SetFloat(tag, value);
                    caseDiagramView1.Invalidate();
                }
                else if (p.ParameterType == CSParameterType.TextAlign)
                {
                    SelectedShape.SetTextAlign(tag, value);
                    caseDiagramView1.Invalidate();
                }
            }
        }