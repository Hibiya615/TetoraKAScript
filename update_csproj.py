import os
import re
import xml.etree.ElementTree as ET

APPDATA = os.getenv('APPDATA')  
KODAKKU_PATH = os.path.join(APPDATA, "XIVLauncherCN", "installedPlugins", "KodakkuAssist")

if not os.path.exists(KODAKKU_PATH):
    print(f"❌ 目录不存在: {KODAKKU_PATH}")
    exit(1)

versions = [d for d in os.listdir(KODAKKU_PATH) if re.match(r'^\d+\.\d+\.\d+\.\d+$', d)]
if not versions:
    print("❌ 未找到任何 KodakkuAssist 版本文件夹")
    exit(1)

latest_version = sorted(versions, key=lambda v: list(map(int, v.split('.'))), reverse=True)[0]
print(f"✅ 发现最新版本: {latest_version}")

#下面改成你的csproj文件名
CSPROJ_FILE = "TetoraKodakkuScript.csproj" 
if not os.path.exists(CSPROJ_FILE):
    print(f"❌ 未找到 .csproj 文件: {CSPROJ_FILE}")
    exit(1)

tree = ET.parse(CSPROJ_FILE)
root = tree.getroot()

ns = {"msbuild": "http://schemas.microsoft.com/developer/msbuild/2003"}
ET.register_namespace('', ns["msbuild"])

for prop_group in root.findall("PropertyGroup", ns):
    kodakku_elem = prop_group.find("KodakkuLibPath", ns)
    if kodakku_elem is not None:
        old_path = kodakku_elem.text
        new_path = f"$(appdata)\\XIVLauncherCN\\installedPlugins\\KodakkuAssist\\{latest_version}\\"
        kodakku_elem.text = new_path
        print(f"🔄 替换路径: \n旧 -> {old_path}\n新 -> {new_path}")
        break

tree.write(CSPROJ_FILE, encoding="utf-8", xml_declaration=True)
print(f"✅ 更新成功: {CSPROJ_FILE}")
