#!/bin/bash
# Legt eine neue Aufgabe an: Konsolenprojekt + xUnit-Testprojekt, registriert in lg2.slnx.
# Fragt alles nacheinander ab. Benötigt bash und dotnet (sonst Docker für den Devcontainer).

set -e
cd "$(dirname "$0")"

# Kein .NET 10 SDK installiert? Dann dieses Skript im Devcontainer ausführen.
if ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
	if [ -n "$IN_DEVCONTAINER" ] || ! command -v docker >/dev/null; then
		echo "❌ Kein .NET 10 SDK gefunden. .NET 10 SDK oder Docker installieren."
		exit 1
	fi
	echo "🐳 Kein .NET 10 SDK gefunden, starte im Devcontainer ..."
	PROJECT="$(basename "$PWD")_devcontainer"
	COMPOSE=(docker compose -p "${PROJECT,,}" -f .devcontainer/docker-compose.yml)
	"${COMPOSE[@]}" up -d app >/dev/null || exit 1
	exec "${COMPOSE[@]}" exec -u vscode -w /workspace -e IN_DEVCONTAINER=1 app "./$(basename "$0")" "$@"
fi

NAMESPACE="Fbs.Lg2"
SOLUTION="lg2.slnx"

# 1. Ordnername
while true; do
	read -rp "Ordnername (z.B. AUFGABE01): " NAME
	if [[ "$NAME" =~ ^[a-zA-Z0-9_-]+$ ]]; then
		break
	fi
	echo "❌ Ungültig. Erlaubt sind nur Buchstaben, Zahlen, _ und -"
done

# 2. Beschreibung
read -rp "Beschreibung: " DESC

# 3. Klassenname (Standard: aus Ordnername, z.B. AUFGABE_01 -> Aufgabe01)
DEFAULT_CLASS=""
IFS='_-' read -ra PARTS <<<"$NAME"
for p in "${PARTS[@]}"; do
	p="${p,,}"
	DEFAULT_CLASS+="${p^}"
done

while true; do
	read -rp "Klassenname (leer lassen für $DEFAULT_CLASS): " CLASSNAME
	CLASSNAME="${CLASSNAME:-$DEFAULT_CLASS}"
	if [[ "$CLASSNAME" =~ ^[A-Za-z][A-Za-z0-9]*$ ]]; then
		break
	fi
	echo "❌ Ungültig. Muss mit einem Buchstaben beginnen, nur Buchstaben und Zahlen."
done

echo ""
echo "🛠  Erstelle '$NAME' (Klasse: $CLASSNAME)..."

APP_DIR="$NAME/$CLASSNAME"
TEST_DIR="$NAME/$CLASSNAME.Tests"
APP_PROJ="$APP_DIR/$CLASSNAME.csproj"
TEST_PROJ="$TEST_DIR/$CLASSNAME.Tests.csproj"

mkdir -p "$APP_DIR" "$TEST_DIR"

# 4. Konsolenprojekt
if [ ! -f "$APP_PROJ" ]; then
	cat <<EOF >"$APP_PROJ"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <Description>$DESC</Description>
  </PropertyGroup>

</Project>
EOF
	echo "  + $APP_PROJ erstellt."
fi

if [ ! -f "$APP_DIR/$CLASSNAME.cs" ]; then
	cat <<EOF >"$APP_DIR/$CLASSNAME.cs"
namespace $NAMESPACE;

public class $CLASSNAME
{
    public string SagHallo()
    {
        return "Hallo aus dem Namespace $NAMESPACE";
    }
}
EOF
	echo "  + $CLASSNAME.cs erstellt."
fi

if [ ! -f "$APP_DIR/Program.cs" ]; then
	cat <<EOF >"$APP_DIR/Program.cs"
using $NAMESPACE;

var aufgabe = new $CLASSNAME();
Console.WriteLine(aufgabe.SagHallo());
EOF
	echo "  + Program.cs erstellt."
fi

# 5. Testprojekt
if [ ! -f "$TEST_PROJ" ]; then
	cat <<EOF >"$TEST_PROJ"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
    <Using Include="Shouldly" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../$CLASSNAME/$CLASSNAME.csproj" />
  </ItemGroup>

</Project>
EOF
	echo "  + $TEST_PROJ erstellt."
fi

if [ ! -f "$TEST_DIR/${CLASSNAME}Tests.cs" ]; then
	cat <<EOF >"$TEST_DIR/${CLASSNAME}Tests.cs"
namespace $NAMESPACE;

public class ${CLASSNAME}Tests
{
    [Fact]
    public void SagHallo_EnthaeltHallo()
    {
        var aufgabe = new $CLASSNAME();
        aufgabe.SagHallo().ShouldContain("Hallo");
    }
}
EOF
	echo "  + ${CLASSNAME}Tests.cs erstellt."
fi

# 6. In Solution registrieren (wie include in settings.gradle)
if ! grep -q "$APP_PROJ" "$SOLUTION"; then
	dotnet sln "$SOLUTION" add --solution-folder "$NAME" "$APP_PROJ" "$TEST_PROJ" >/dev/null
	echo "  + In $SOLUTION registriert."
fi

echo "✅ Fertig. Tests laufen lassen: ./test.sh $NAME"
