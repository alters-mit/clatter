import re
from typing import Dict, List, Union
from subprocess import call
from pathlib import Path
import xml.etree.ElementTree as ET
from xml.etree.ElementTree import ElementTree, Element
from markdown import markdown


def get_description(e: Element, brief_description: bool = True, detailed_description: bool = True) -> str:
    """
    :param e: The XML element.
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
            # Replace words with links.
            de_text: str = ET.tostring(e.find(desc), encoding="utf-8", method="text").decode()
            for link in internal_links:
                li: str = link[1]
                de_text = re.sub(r"\b" + li + r"\b", f'[`{li}`]({li}.html)', de_text, flags=re.MULTILINE)
            external_links = re.findall(r'<ulink url="(.*?)">(.*?)</ulink>', de_text_raw, flags=re.MULTILINE)
            for link in external_links:
                li: str = link[0]
                de_text = de_text.replace(li, f'[{li.split("/")[-1]}]({li})')
            descriptions.append(de_text.strip())
    return " ".join(descriptions)


def get_type(e: Element) -> str:
    """
    :param e: An XML element.

    :return: A value type.
    """

    ref: Element = e.find("ref")
    if ref is None:
        t = e.text
    else:
        t = f"[`{ref.text}`]({ref.text}.html)"
    return re.sub(r"(.*?)<\s(.*?)\s>", r"\1<\2>", t)


class EnumDef:
    def __init__(self, name: str, e: Element):
        self.name: str = name
        member_def: Element = e.find("memberdef")
        self.description: str = get_description(e=member_def)
        self.values: Dict[str, str] = dict()
        for enum_value in member_def.findall("enumvalue"):
            self.values[enum_value.find("name").text] = enum_value.find("initializer").text.split("=")[1].strip()


class Field:
    """
    A field definition.
    """

    def __init__(self, e: Element):
        """
        :param e: The XML element.
        """

        self.name: str = e.find("name").text
        self.type: str = get_type(e.find("type"))
        ty = e.find("type").text
        self.readonly: bool = ty is not None and ty.startswith("readonly")
        if self.type.startswith("const"):
            self.const: bool = True
            self.type = self.type.split("const")[1].strip()
        else:
            self.const = False
        self.description: str = get_description(e)
        initializer: Element = e.find("initializer")
        self.default_value: str = ""
        if initializer is not None:
            self.default_value = ET.tostring(initializer, encoding="utf-8", method="text").decode().split("=")[1].strip()
        self.static: bool = e.attrib["static"] == "yes"
        self.protection: str = e.attrib["prot"]


class Property(Field):
    """
    A property definition.
    """

    def __init__(self, e: Element):
        """
        :param e: The XML element.
        """

        super().__init__(e=e)
        self.gettable: bool = e.attrib["gettable"] == "yes"
        self.settable: bool = e.attrib["settable"] == "yes"


class Parameter:
    """
    A function parameter.
    """

    def __init__(self, type: str, name: str, description: str):
        """
        :param type: The parameter type.
        :param name: The parameter name.
        :param description: The parameter description.
        """

        self.type: str = type
        self.name: str = name
        self.description: str = description


class Method:
    """
    A function definition.
    """

    def __init__(self, class_name: str, e: Element):
        """
        :param class_name: The class name. This is used to find constructors.
        :param e: The XML element.
        """

        self.type: str = e.find("type").text
        if self.type is None:
            self.type = "void"
        if "override" in self.type:
            self.override: bool = True
            self.type = self.type.split("override")[1].strip()
        else:
            self.override = False
        if "const " in self.type:
            self.const: bool = True
            self.type = self.type.split("const")[1].strip()
        else:
            self.const = False
        self.name: str = e.find("name").text
        self.description = get_description(e, detailed_description=False)
        self.args_string: str = e.find("argsstring").text
        self.static: bool = e.attrib["static"] == "yes"
        self.protection: str = e.attrib["prot"]
        self.virtual: bool = e.attrib["virt"] == "virtual"
        self.constructor: bool = class_name == self.name
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
                                             type=parameters[pa],
                                             description=parameter_descriptions[pa] if pa in parameter_descriptions else ""))


class Klass:
    """
    A class definition.
    """

    IGNORE_SECTIONS: List[str] = ["private-attrib", "protected-attrib", "protected-func", "private-static-attrib",
                                  "private-func", "private-static-func", "private-type"]

    def __init__(self, name: str, namespace: str, et: ElementTree):
        self.name: str = name
        self.namespace: str = namespace
        cd: Element = et.find("compounddef")
        self.is_class: bool = cd.attrib["id"].startswith("class")
        self.abstract: bool = cd.attrib["abstract"] == "yes" if "abtract" in cd.attrib else False
        self.description: str = get_description(cd)
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
    for namespace in namespaces:
        # Add a title.
        ns_lower = namespace.lower()
        sidebar += f'\t\t\t\t<a class="title" href="{ns_lower}/overview.md">{namespace}</a>\n\n'
        # Add a link to each file.
        for f in namespaces[namespace]:
            sidebar += f'\t\t\t\t<a class="section" href="{ns_lower}/{f}.html">{f}</a>\n'
        sidebar += f'\n\t\t\t\t<div class="divider left"></div>\n\n'
    sidebar = f"\t\t{sidebar.strip()}\n\t\t\t</div>"
    return sidebar


def get_html_prefix() -> str:
    q = Path("html_prefix.txt").read_text(encoding="utf-8")
    q += sidebar + '\n\t\t<div class="right-col">\n\n'
    return q


def get_html_suffix() -> str:
    """
    :return: The suffix of an HTML page.
    """

    return "</div></div></div></body></html>"


def get_overview(namespace: str) -> str:
    """
    :param namespace: The namespace.

    :return: The HTML overview page of the namespace.
    """

    md: str = Path(f"{namespace.lower()}/overview.md").resolve().read_text(encoding="utf-8")
    return get_html_prefix() + markdown(md) + get_html_suffix()


def doxygen() -> None:
    """
    Call doxygen to generate XML document files.
    """

    call("doxygen")


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
            return EnumDef(name=name, e=section_def)
        else:
            raise Exception(ET.tostring(compound_def).decode())


# doxygen()
namespaces = get_namespaces()
sidebar = get_sidebar()
dst = Path("html")
dst.joinpath("clatter.core/overview.html").resolve().write_text(get_overview("Clatter.Core"))
for ns in namespaces:
    for kl in namespaces[ns]:
        get_klass(name=kl, namespace=ns)
