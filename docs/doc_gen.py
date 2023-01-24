from copy import copy
import re
from typing import Dict, List, Union, Tuple
from subprocess import call, DEVNULL
from pathlib import Path
from shutil import rmtree
import xml.etree.ElementTree as ET
from xml.etree.ElementTree import ElementTree, Element
from markdown import markdown


def get_version() -> str:
    """
    :return: The version number of Clatter.Core.
    """

    assembly_info: str = Path("../Clatter/Clatter.Core/Properties/AssemblyInfo.cs").read_text(encoding="utf-8")
    return re.search(r'^\[assembly: AssemblyVersion\("(.*?)"\)\]', assembly_info, flags=re.MULTILINE).group(1)


def get_description(e: Element, is_paragraphs: bool, brief_description: bool = True, detailed_description: bool = True) -> str:
    """
    :param e: The XML element.
    :param is_paragraphs: If True, include paragraph HTML tags and line breaks.
    :param brief_description: If True, try to include the brief description.
    :param detailed_description: If True, try to include the detailed description.

    :return: Description text.
    """

    descriptions: List[str] = list()
    keys = []
    if brief_description:
        keys.append("briefdescription")
    if detailed_description:
        keys.append("detaileddescription")
    for desc in keys:
        de_el: Element = e.find(desc)
        if de_el is not None:
            # Get the raw text to get internal links.
            de_text_raw: str = ET.tostring(e.find(desc), encoding="utf-8", method="html").decode()
            internal_links = re.findall(r'<computeroutput><ref kindref="compound" refid="(.*?)">(.*?)</ref></computeroutput>',
                                        de_text_raw, flags=re.MULTILINE)
            de_text: str = ET.tostring(e.find(desc), encoding="utf-8", method="text").decode()
            # Generate a list.
            if "<itemizedlist>" in de_text_raw:
                itemized_list = re.search(r"<itemizedlist>((.|\n)*?)</itemizedlist>", de_text_raw, flags=re.MULTILINE).group(1)
                list_items = re.findall(r"<listitem><para>(.*?)</para>[.|\n]</listitem>", itemized_list)
                for lii in list_items:
                    de_text = de_text.replace(lii.replace("<computeroutput>", "").replace("</computeroutput>", ""),
                                              f"- {lii.strip()}")
            lis = []
            for link in internal_links:
                li: str = link[1]
                if li in lis:
                    continue
                lis.append(li)
                de_text = re.sub(r"\b" + li + r"\b", f'[`{li}`]({li.split(".")[-1]}.html)', de_text, flags=re.MULTILINE)
            external_links = re.findall(r'<ulink url="(.*?)">(.*?)</ulink>', de_text_raw, flags=re.MULTILINE)
            for link in external_links:
                li: str = link[0]
                de_text = de_text.replace(li, f'[{li.split("/")[-1]}]({li})')
            if is_paragraphs:
                de_text = markdown(de_text.strip().replace("\n\n", "%%").replace("\n", "\n\n").replace("%%", "\n\n"))
            else:
                de_text = de_text.strip()
            descriptions.append(de_text)
    return " ".join(descriptions)


def get_description_and_code_examples(description: str) -> Tuple[str, str]:
    """
    Replace code_example tags in a description with actual code examples.

    :return: Tuple: The modified description and the code examples.
    """

    # Add code examples.
    while "code_example" in description:
        match = re.search(r"{code_example:(.*?)}", description, flags=re.MULTILINE)
        code_example_filename = match.group(1)
        code_example = Path(f"../Clatter/doc_code_examples/{code_example_filename}.cs").resolve().read_text(encoding="utf-8-sig")
        code_example = "<pre><code>" + code_example + "</code></pre>\n\n"
        desc_split = description.split(match.group(0))
        if len(desc_split[1].strip()) > 0:
            code_example = "</p>\n" + code_example + "<p>"
        description = description.replace(match.group(0), code_example)
    desc_split = description.split("<p>Code Examples</p>")
    description = desc_split[0].strip()
    # Get code examples.
    if len(desc_split) > 1:
        code_examples = "<h2>Code Examples</h2>\n\n" + desc_split[1]
    else:
        code_examples = ""
    return description, code_examples


def get_type(e: Element) -> str:
    """
    :param e: An XML element.

    :return: A value type.
    """

    ref: Element = e.find("ref")
    if ref is None:
        t = e.text
    elif "AudioGenerationAction" in ref.text:
        t = ref.text
    else:
        t = f'<a href="{ref.text}.html"><code>{ref.text}</code></a>'
    return t


class EnumDef:
    """
    A definition for an enum type.
    """

    def __init__(self, name: str, namespace: str, e: Element):
        """
        :param name: The name of the enum type.
        :param namespace: The namespace this type belongs to.
        :param e: The root XML element.
        """

        self.name: str = name
        self.namespace: str = namespace
        member_def: Element = e.find("memberdef")
        self.description: str = get_description(e=member_def, is_paragraphs=True)
        self.values: List[str] = list()
        self.value_descriptions: Dict[str, str] = dict()
        for enum_value in member_def.findall("enumvalue"):
            value_name: str = enum_value.find("name").text
            self.values.append(value_name)
            value_description_element: Element = enum_value.find("briefdescription")
            value_description = ET.tostring(value_description_element, encoding="utf-8", method="text").decode().strip()
            if len(value_description) > 0:
                self.value_descriptions[value_name] = value_description

    def html(self) -> str:
        # Get the header.
        html = f"<h1>{self.name}</h1>\n\n"
        html += f'<p class="subtitle">enum in {self.namespace}</p>\n\n'
        # Add the description.
        description = self.description.replace("\n\n", "%%").replace("\n", "\n\n").replace("%%", "\n\n")
        description, code_examples = get_description_and_code_examples(description)
        html += markdown(f'{description.strip()}\n\n')
        # There are no value descriptions.
        if len(self.value_descriptions) == 0:
            table = "<table>\n\t<tr>\n\t\t<th><strong>Value</strong></th>\n\t</tr>\n"
            for value in self.values:
                table += f"\t<tr>\n\t\t<th>{value}</th>\n\t</tr>\n"
        else:
            table = "<table>\n\t<tr>\n\t\t<th><strong>Value</strong></th>\n\t\t<th><strong>Description</strong></th>\n\t</tr>\n"
            for value in self.values:
                if value in self.value_descriptions:
                    value_description = self.value_descriptions[value]
                else:
                    value_description = ""
                table += f"\t<tr>\n\t\t<th>{value}</th>\n\t\t<th>{value_description}</th>\n\t</tr>\n"
        table += "\n</table>"
        html += table
        if len(code_examples) > 0:
            html += "\n\n" + code_examples
        return html.strip()


class Field:
    """
    A field definition.
    """

    def __init__(self, e: Element):
        """
        :param e: The root XML element.
        """

        self.name: str = e.find("name").text
        self.type: str = get_type(e.find("type"))
        ty = e.find("type").text
        self.readonly: bool = ty is not None and ty.startswith("readonly")
        self.type = self.type.replace("readonly", "").strip()
        if self.type.startswith("const"):
            self.const: bool = True
            self.type = self.type.split("const")[1].strip()
        else:
            self.const = False
        self.description: str = markdown(get_description(e=e, is_paragraphs=False)).replace("<p>", "").replace("</p>", "")
        initializer: Element = e.find("initializer")
        self.default_value: str = ""
        if initializer is not None:
            self.default_value = ET.tostring(initializer, encoding="utf-8", method="text").decode().split("=")[1].strip()
        self.static: bool = e.attrib["static"] == "yes"
        self.protection: str = e.attrib["prot"]
        self.inherited_from: str = ""


class Property(Field):
    """
    A property definition.
    """

    def __init__(self, e: Element):
        """
        :param e: The root XML element.
        """

        super().__init__(e=e)
        self.gettable: bool = e.attrib["gettable"] == "yes"
        self.settable: bool = e.attrib["settable"] == "yes"


class Parameter:
    """
    A function parameter.
    """

    def __init__(self, parameter_type: str, name: str, description: str):
        """
        :param parameter_type: The parameter type.
        :param name: The parameter name.
        :param description: The parameter description.
        """

        self.parameter_type: str = parameter_type
        self.name: str = name
        self.description: str = description


class Method:
    """
    A function definition.
    """

    def __init__(self, class_name: str, e: Element):
        """
        :param class_name: The class name. This is used to find constructors.
        :param e: The root XML element.
        """

        self.type: str = e.find("type").text
        if self.type is None:
            self.type = "void"
        if "const " in self.type:
            self.const: bool = True
            self.type = self.type.split("const")[1].strip()
        else:
            self.const = False
        self.name: str = e.find("name").text
        self.description = get_description(e, detailed_description=False, is_paragraphs=True)
        self.args_string: str = e.find("argsstring").text
        self.static: bool = e.attrib["static"] == "yes"
        self.protection: str = e.attrib["prot"]
        self.virtual: bool = e.attrib["virt"] == "virtual"
        self.constructor: bool = class_name == self.name
        self.inherited_from: str = ""
        parameters: Dict[str, str] = dict()
        parameter_elements: List[Element] = e.findall("param")
        for pe in parameter_elements:
            parameter_type_element: Element = pe.find("type")
            parameter_ref_element: Element = parameter_type_element.find("ref")
            if parameter_ref_element is not None:
                parameter_type = f"[`{parameter_ref_element.text}`]({parameter_ref_element.text}.html)"
            else:
                parameter_type = parameter_type_element.text
            parameters[pe.find("declname").text] = parameter_type
        parameter_descriptions: Dict[str, str] = dict()
        detailed_description: Element = e.find("detaileddescription")
        if detailed_description is not None:
            ps: List[str] = ET.tostring(detailed_description, encoding="utf-8", method="text").decode().split("\n")
            ps = [line for line in ps if len(line.strip()) > 0]
            for i in range(0, len(ps), 2):
                parameter_descriptions[ps[i]] = ps[i + 1]
        self.parameters: List[Parameter] = list()
        for pa in parameters:
            self.parameters.append(Parameter(name=pa,
                                             parameter_type=parameters[pa],
                                             description=parameter_descriptions[pa] if pa in parameter_descriptions else ""))

    def html(self) -> str:
        """
        :return: The HTML string of the method's documentation.
        """

        text = f'<h3>{self.name}</h3>\n\n'
        if self.constructor:
            text += f"<p><code>public {self.name}{self.args_string}</code></p>\n\n"
        elif self.static:
            text += f"<p><code>public static {self.type} {self.name}{self.args_string}</code></p>\n\n"
        else:
            text += f"<p><code>public {self.type} {self.name}{self.args_string}</code></p>\n\n"
        if not self.constructor and len(self.description.strip()) > 0:
            text += f'{markdown(self.description.strip())}\n\n'
        # Add a mention re: inheritance.
        if self.inherited_from != "":
            text += f'<p>Inherited from <a href="{self.inherited_from}.html"><code>{self.inherited_from}</code></a>.</p>\n\n'
        if len(self.parameters) > 0:
            table = "<table>\n\t<tr>\n\t\t<th><strong>Name</strong></th><th>\n\t\t<strong>Type</strong></th>\n\t\t<th><strong>Description</strong></th>\n\t</tr>\n"
            for parameter in self.parameters:
                t = markdown(parameter.parameter_type).replace("<p>", "").replace("</p>", "")
                table += f"\t<tr>\n\t\t<th>{parameter.name}</th>\n\t\t<th>{t}</th>\n\t\t<th>{parameter.description}</th>\n\t</tr>"
            table += "\n</table>"
            text += table + "\n\n"
        return text


class Klass:
    """
    A class definition.
    """

    IGNORE_SECTIONS: List[str] = ["private-attrib", "protected-attrib", "protected-func", "private-static-attrib",
                                  "private-func", "private-static-func", "private-type"]

    def __init__(self, name: str, namespace: str, et: ElementTree):
        """
        :param name: The name of the class type.
        :param namespace: The namespace this type belongs to.
        :param et: The XML element tree.
        """

        self.name: str = name
        self.namespace: str = namespace
        cd: Element = et.find("compounddef")
        self.is_class: bool = cd.attrib["id"].startswith("class")
        self.abstract: bool = cd.attrib["abstract"] == "yes" if "abstract" in cd.attrib else False
        self.description: str = get_description(e=cd, is_paragraphs=True)
        self.public_static_fields: List[Field] = list()
        self.public_fields: List[Field] = list()
        self.public_static_methods: List[Method] = list()
        self.public_methods: List[Method] = list()
        self.properties: List[Property] = list()
        # Get sections.
        section_defs: List[Element] = cd.findall("sectiondef")
        for sd in section_defs:
            section_kind: str = sd.attrib["kind"]
            if section_kind == "public-static-attrib":
                self.public_static_fields.extend(Klass.get_fields(sd))
            elif section_kind == "public-attrib":
                self.public_fields.extend(Klass.get_fields(sd))
            elif section_kind == "public-static-func":
                self.public_static_methods.extend(Klass.get_methods(name, sd))
            elif section_kind == "public-func":
                self.public_methods.extend(Klass.get_methods(name, sd))
            elif section_kind == "property":
                self.properties.extend(Klass.get_properties(sd))
            elif section_kind in Klass.IGNORE_SECTIONS:
                continue
            else:
                raise Exception(name, section_kind)
        # Get the lineage.
        self.inheritance: List[str] = list()
        if self.is_class:
            inheritance_graph: Element = cd.find("inheritancegraph")
            if inheritance_graph is not None:
                # Get my ID.
                nodes = inheritance_graph.findall("node")
                node_id: str = ""
                for node in nodes:
                    inheritance_name = node.find("label").text.split(".")[-1]
                    if inheritance_name == self.name:
                        node_id = node.attrib["id"]
                        break
                for node in inheritance_graph.findall("node"):
                    inheritance_name = node.find("label").text.split(".")[-1]
                    if inheritance_name == self.name:
                        continue
                    child_node = node.find("childnode")
                    # Only add child nodes if this klass is the child.
                    if child_node is not None and child_node.attrib["refid"] == node_id:
                        continue
                    if inheritance_name is not None:
                        self.inheritance.append(inheritance_name)

    def html(self) -> str:
        """
        :return: The string of the class's HTML document.
        """

        # Get the header.
        html = f"<h1>{self.name}</h1>\n\n"
        # Get the subtitle.
        if self.is_class:
            if self.abstract:
                member_type = "abstract class"
            else:
                member_type = "class"
        else:
            member_type = "struct"
        html += f'<p class="subtitle">{member_type} in {self.namespace}</p>\n\n'
        if len(self.inheritance) > 0:
            inheritance_p = f'<p class="subtitle">Inherits from '
            for inh in self.inheritance:
                inheritance_p += f'<a href="{inh}.html"><code>{inh}</code></a>, '
            html += inheritance_p[:-2] + f'</p>\n\n'
        # Add the description.
        description = self.description.replace("\n\n", "%%").replace("\n", "\n\n").replace("%%", "\n\n")
        description, code_examples = get_description_and_code_examples(description)
        html += markdown(f'{description.strip()}\n\n')
        constants = [f for f in self.public_static_fields if f.const]
        static_fields = [f for f in self.public_static_fields if not f.const]
        # Add members.
        if len(constants) > 0:
            html += f'<h2>Constants</h2>\n\n'
            html += Klass.get_fields_table(fields=constants) + "\n\n"
        if len(static_fields) > 0:
            html += f'<h2>Static Fields</h2>\n\n'
            html += Klass.get_fields_table(fields=static_fields) + "\n\n"
        if len(self.public_fields) > 0:
            html += f'<h2>Fields</h2>\n\n'
            html += Klass.get_fields_table(fields=self.public_fields) + "\n\n"
        if len(self.properties) > 0:
            html += f'<h2>Properties</h2>\n\n'
            table = "<table>\n\t<tr>\n\t\t<th><strong>Name</strong></th><th>\n\t\t<strong>Type</strong></th>\n\t\t<th><strong>Description</strong></th>\n\t\t<th><strong>Get/Set</strong></th>\n\t</tr>\n"
            for field in self.properties:
                get_set = []
                if field.gettable:
                    get_set.append("get")
                if field.settable:
                    get_set.append("set")
                table += f"\t<tr>\n\t\t<th>{field.name}</th>\n\t\t<th>{field.type}</th>\n\t\t<th>{field.description}</th>\n\t\t<th>{' '.join(get_set)}</th>\n\t</tr>\n"
            table += "\n</table>"
            html += table + "\n\n"
        if len(self.public_static_methods) > 0:
            html += f'<h2>Static Methods</h2>\n\n'
            for method in self.public_static_methods:
                html += method.html()
        if len(self.public_methods) > 0:
            html += f'<h2>Methods</h2>\n\n'
            for method in self.public_methods:
                html += method.html()
        if len(code_examples) > 0:
            html += "\n\n" + code_examples
        return html.strip()

    @staticmethod
    def get_fields_table(fields: List[Field]) -> str:
        """
        :param fields: A list of field.

        :return: An HTML string of a table of fields.
        """

        table = "<table>\n\t<tr>\n\t\t<th><strong>Name</strong></th><th>\n\t\t<strong>Type</strong></th>\n\t\t<th><strong>Description</strong></th>\n\t\t<th><strong>Default Value</strong></th>\n\t</tr>\n"
        for field in fields:
            if field.readonly and not field.const and "Readonly." not in field.description:
                field.description += " Readonly."
            # Add a mention re: inheritance.
            if field.inherited_from != "":
                field.description += f' Inherited from <a href="{field.inherited_from}.html"><code>{field.inherited_from}</code></a>.\n\n'
            table += f"\t<tr>\n\t\t<th>{field.name}</th>\n\t\t<th>{field.type}</th>\n\t\t<th>{field.description}</th>\n\t\t<th>{field.default_value}</th>\n\t</tr>\n"
        table += "</table>"
        return table

    @staticmethod
    def get_fields(e: Element) -> List[Field]:
        """
        :param e: The root XML element.

        :return: A list of fields.
        """

        fields: List[Field] = list()
        members: List[Element] = e.findall("memberdef")
        for member in members:
            member_kind = member.attrib["kind"]
            if member_kind == "variable":
                fields.append(Field(member))
            else:
                raise Exception(ET.tostring(e))
        return fields

    @staticmethod
    def get_methods(class_name: str, e: Element) -> List[Method]:
        """
        :param class_name: The class name.
        :param e: The root XML element.

        :return: A list of methods.
        """

        methods: List[Method] = list()
        members: List[Element] = e.findall("memberdef")
        for member in members:
            member_kind = member.attrib["kind"]
            if member_kind == "function":
                methods.append(Method(class_name, member))
            else:
                raise Exception(ET.tostring(e))
        return methods

    @staticmethod
    def get_properties(e: Element) -> List[Property]:
        """
        :param e: The root XML element.

        :return: A list of properties.
        """

        properties: List[Property] = list()
        members: List[Element] = e.findall("memberdef")
        for member in members:
            member_kind = member.attrib["kind"]
            if member_kind == "property":
                properties.append(Property(member))
            else:
                raise Exception(ET.tostring(e))
        return properties


def get_namespaces() -> Dict[str, List[str]]:
    """
    :return: A dictionary. Key = Namespace. Value = A list of names of classes in the namespace.
    """

    namespaces: Dict[str, List[str]] = dict()
    for namespace in ["Clatter.Core", "Clatter.Unity"]:
        namespaces[namespace] = list()
        src = Path(f"../Clatter/{namespace}").resolve()
        for f in src.iterdir():
            if f.suffix == ".cs":
                namespaces[namespace].append(f.stem)
    return namespaces


def get_sidebar() -> str:
    """
    :return: The HTML for the sidebar div.
    """

    sidebar = '<div class="sidepanel">\n'
    sidebar += '\t\t\t\t<a class="title" href="overview.html">Overview</a>\n\n'
    sidebar += f'\n\t\t\t\t<div class="divider left"></div>\n\n'
    for namespace in namespaces:
        # Add a title.
        ns_lower = namespace.lower()
        sidebar += f'\t\t\t\t<a class="title" href="{ns_lower}_overview.html">{namespace}</a>\n\n'
        # Add a link to each file.
        for f in namespaces[namespace]:
            sidebar += f'\t\t\t\t<a class="section" href="{f}.html">{f}</a>\n'
        sidebar += f'\n\t\t\t\t<div class="divider left"></div>\n\n'
    sidebar += '\t\t\t\t<a class="title" href="cli_overview.html">Clatter CLI</a>\n\n'
    sidebar += f'\n\t\t\t\t<div class="divider left"></div>\n\n'
    sidebar += '\t\t\t\t<a class="title" href="benchmark.html">Benchmark</a>\n\n'
    sidebar = f"\t\t{sidebar.strip()}\n\t\t\t</div>"
    return sidebar


def get_html_prefix() -> str:
    """
    :return: The prefix string for an HTML page.
    """

    q = Path("html_prefix.txt").read_text(encoding="utf-8")
    q += sidebar + '\n\t\t<div class="right-col">\n\n'
    return q


def get_html_suffix() -> str:
    """
    :return: The suffix of an HTML page.
    """

    return "</div></div></div></body></html>"


def get_clatter_core_overview() -> str:
    """
    :return: The HTML for the Clatter.Core overview doc.
    """

    version = get_version()
    text: str = markdown(Path(f"clatter.core.md").resolve().read_text(encoding="utf-8"))
    # Add the download links.
    downloads = '\n\n<p><strong>Download:</strong></p>\n\n'
    downloads += f'<p><a href="https://github.com/alters-mit/clatter/releases/download/{version}/Clatter.Core.dll">Clatter.Core.dll</a></p>'
    text = text.replace("<p>[URLS]</p>", downloads)
    return get_html_prefix() + text + get_html_suffix()


def get_clatter_unity_overview() -> str:
    """
    :return: The HTML for the Clatter.Core overview doc.
    """

    version = get_version()
    text: str = markdown(Path(f"clatter.unity.md").resolve().read_text(encoding="utf-8"))
    # Add the download links.
    downloads = '\n\n<p><strong>Download:</strong></p>\n\n'
    downloads += f'<p><a href="https://github.com/alters-mit/clatter/releases/download/{version}/Clatter.Core.dll">Clatter.Core.dll</a>&emsp;'
    downloads += f'<a href="https://github.com/alters-mit/clatter/releases/download/{version}/Clatter.Unity.dll">Clatter.Unity.dll</a>'
    downloads += "</p>"
    text = text.replace("<p>[URLS]</p>", downloads)
    return get_html_prefix() + text + get_html_suffix()


def get_readme() -> str:
    """
    :return: The HTML for the overall overview document.
    """

    md: str = Path("overview.md").resolve().read_text(encoding="utf-8")
    return get_html_prefix() + markdown(md) + get_html_suffix()


def get_benchmark() -> str:
    """
    :return: The HTML for the benchmark document.
    """

    md = Path("benchmark.md").read_text()
    return get_html_prefix() + markdown(md, extensions=['markdown.extensions.tables']) + get_html_suffix()


def doxygen() -> None:
    """
    Call doxygen to generate XML document files.
    """

    call("doxygen", stdout=DEVNULL)


def snake_case(camel_case: str) -> str:
    """
    :param camel_case: A CamelCase string.

    :return: A snake_case string.
    """

    # Source: https://stackoverflow.com/a/1176023
    return re.sub(r'(?<!^)(?=[A-Z])', '_', camel_case).lower()


def get_klass(name: str, namespace: str) -> Union[Klass, EnumDef]:
    d = Path("xml")
    # Get the filename from the 8cs file.
    root = ET.parse(d.joinpath(f"_{snake_case(name)}_8cs.xml").resolve().absolute())
    compound_def: Element = root.find("compounddef")
    inner_class: Element = compound_def.find("innerclass")
    if inner_class is not None:
        filename: str = inner_class.attrib["refid"] + ".xml"
        # Load the actual file.
        root: ElementTree = ET.parse(d.joinpath(filename).resolve().absolute())
        return Klass(name=name, namespace=namespace, et=root)
    else:
        section_def: Element = compound_def.find("sectiondef")
        # This is an enum.
        if section_def.attrib["kind"] == "enum":
            return EnumDef(name=name, namespace=namespace, e=section_def)
        else:
            raise Exception(ET.tostring(compound_def).decode())


def inherit_fields(parent_class_name: str, child_fields: List[Field], parent_fields: List[Field]) -> List[Field]:
    """
    Copy inherited fields from a parent class into a child class.

    :param parent_class_name: The name of the parent class.
    :param child_fields: A list of the child class's fields. This will be added to.
    :param parent_fields: A list of the parent class's fields.

    :return: The list of the child class's fields, including inherited methods.
    """

    for field in parent_fields:
        fi = copy(field)
        fi.inherited_from = parent_class_name
        child_fields.append(fi)
    return child_fields


def inherit_methods(parent_class_name: str, child_methods: List[Method], parent_methods: List[Method]) -> List[Method]:
    """
    Copy inherited methods from a parent class into a child class.

    :param parent_class_name: The name of the parent class.
    :param child_methods: A list of the child class's methods. This will be added to.
    :param parent_methods: A list of the parent class's methods.

    :return: The list of the child class's methods, including inherited methods.
    """

    # Get a list of existing method names.
    method_names = [m.name for m in child_methods]
    for method in parent_methods:
        # If a method by the same name already exists, it means we've overridden a method.
        if method.name in method_names:
            # Mark the overridden method as inherited.
            for q in range(len(child_methods)):
                if child_methods[q].name == method.name:
                    child_methods[q].inherited_from = parent_class_name
                    break
            continue
        # Copy the inherited method.
        me = copy(method)
        me.inherited_from = parent_class_name
        child_methods.append(me)
    return child_methods


def class_inheritance() -> None:
    """
    Copy inherited fields and methods into child classes and generate documentation.
    """

    # Figure out inheritance.
    for klass_name in klasses:
        # Get my inheritance.
        for parent_name in klasses[klass_name].inheritance:
            # This can happen with classes outside the namespace e.g. MonoBehaviour.
            if parent_name not in klasses:
                continue
            parent_klass = klasses[parent_name]
            # Copy the fields.
            klasses[klass_name].public_static_fields = inherit_fields(parent_class_name=parent_klass.name,
                                                                      child_fields=klasses[klass_name].public_static_fields,
                                                                      parent_fields=parent_klass.public_static_fields)
            klasses[klass_name].public_fields = inherit_fields(parent_class_name=parent_klass.name,
                                                               child_fields=klasses[klass_name].public_fields,
                                                               parent_fields=parent_klass.public_fields)
            # Copy the methods.
            klasses[klass_name].public_static_methods = inherit_methods(parent_class_name=parent_klass.name,
                                                                        child_methods=klasses[klass_name].public_static_methods,
                                                                        parent_methods=parent_klass.public_static_methods)
            klasses[klass_name].public_methods = inherit_methods(parent_class_name=parent_klass.name,
                                                                 child_methods=klasses[klass_name].public_methods,
                                                                 parent_methods=parent_klass.public_methods)
            # Copy the properties.
            for parent_property in parent_klass.properties:
                pr = copy(parent_property)
                pr.inherited_from = parent_klass.name
                klasses[klass_name].properties.append(pr)
        # Generate the doc.
        klass_html = klasses[klass_name].html()
        klass_html = get_html_prefix() + klass_html + get_html_suffix()
        dst.joinpath(klass_name + ".html").write_text(klass_html)


def get_cli() -> str:
    """
    :return: The HTML for the CLI documentation.
    """

    version = get_version()
    text: str = markdown(Path("cli.md").resolve().read_text(encoding="utf-8").replace("powershell", ""))
    # Add the download links.
    downloads = '\n\n<p><strong>Download:</strong></p>\n\n<p>'
    for platform, exe in zip(["Linux", "OSX", "Windows"], ["clatter_linux", "clatter_osx", "clatter.exe"]):
        downloads += f'<a href="https://github.com/alters-mit/clatter/releases/download/{version}/{exe}">{platform}</a>&emsp;'
    downloads += "</p>"
    text = text.replace("<p>[URLS]</p>", downloads)
    return get_html_prefix() + text.strip() + get_html_suffix()


# Generate XML with Doxygen.
doxygen()
# Get the namespaces.
namespaces = get_namespaces()
# Get the sidebar html.
sidebar = get_sidebar()
dst = Path("html/html").resolve()
# Remove the existing docs.
if dst.exists():
    rmtree(str(dst))
dst.mkdir(parents=True)
# Write the overview doc.
dst.joinpath("overview.html").write_text(get_readme())
# Add the overview docs.
dst.joinpath("cli_overview.html").write_text(get_cli())
dst.joinpath("benchmark.html").write_text(get_benchmark())
dst.joinpath("clatter.core_overview.html").write_text(get_clatter_core_overview())
dst.joinpath("clatter.unity_overview.html").write_text(get_clatter_unity_overview())
# Add the API docs.
for ns in namespaces:
    # Generate class, struct, and enum docs.
    klasses: Dict[str, Klass] = dict()
    for kl in namespaces[ns]:
        klass = get_klass(name=kl, namespace=ns)
        if isinstance(klass, Klass):
            # Append this class to review class inheritance later.
            if klass.is_class:
                klasses[klass.name] = klass
            # This is a struct. Generate the doc.
            else:
                html = klass.html()
                html = get_html_prefix() + html + get_html_suffix()
                dst.joinpath(klass.name + ".html").write_text(html)
        # This is an enum. Generate the doc.
        elif isinstance(klass, EnumDef):
            html = klass.html()
            html = get_html_prefix() + html + get_html_suffix()
            dst.joinpath(klass.name + ".html").write_text(html)
    # Figure out inheritance.
    class_inheritance()
# Remove the XML.
rmtree(Path("xml").absolute())
