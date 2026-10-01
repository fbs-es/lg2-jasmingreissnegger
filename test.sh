#!/bin/bash
# Fuehrt die Tests aller Aufgaben aus (wie ./gradlew checkAllTasks).
#
#   ./test.sh              alle Aufgaben testen
#   ./test.sh AUFGABE01    nur eine Aufgabe testen
#   ./test.sh --list       alle Aufgaben mit Beschreibung auflisten

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

LINE=$(printf '=%.0s' {1..60})

# Beschreibung aus dem <Description>-Tag der Projekte einer Aufgabe lesen
description() {
	local desc
	desc=$(sed -n 's:.*<Description>\(.*\)</Description>.*:\1:p' "$1"/*/*.csproj 2>/dev/null | head -n1)
	echo "${desc:----}"
}

# Zahl hinter "<label>:" aus der dotnet test Ausgabe holen
count() {
	sed -n "s/^ *$1: *\([0-9]*\).*/\1/p" <<<"$2" | head -n1
}

if [ "$1" = "--list" ]; then
	echo ""
	echo "--------------------- Aktuelle Aufgaben ---------------------"
	for dir in */; do
		dir="${dir%/}"
		ls "$dir"/*/*.csproj >/dev/null 2>&1 || continue
		printf -- "- %-25s | %s\n" "$dir" "$(description "$dir")"
	done
	echo "$LINE"
	exit 0
fi

if [ -n "$1" ]; then
	if [ ! -d "$1" ]; then
		echo "❌ Aufgabe '$1' nicht gefunden."
		exit 1
	fi
	PROJECTS=$(ls "${1%/}"/*/*.Tests.csproj 2>/dev/null)
else
	PROJECTS=$(ls */*/*.Tests.csproj 2>/dev/null)
fi

if [ -z "$PROJECTS" ]; then
	echo "❌ Keine Testprojekte gefunden. Neue Aufgabe anlegen mit ./neu.sh"
	exit 1
fi

TOTAL=0
PASSED=0
FAILED=0
FAILED_TASKS=()

for proj in $PROJECTS; do
	task="${proj%%/*}"
	echo "🧪 Teste $task ..."

	output=$(dotnet test "$proj" --nologo --logger "console;verbosity=normal" 2>&1)
	status=$?

	# Einzelne Testergebnisse zeigen, bei Fehlern die komplette Ausgabe
	if [ $status -eq 0 ]; then
		grep -E '^ +(Passed|Skipped) ' <<<"$output"
	else
		echo "$output"
	fi

	t=$(count "Total tests" "$output")
	p=$(count "Passed" "$output")
	f=$(count "Failed" "$output")

	if [ $status -eq 0 ]; then
		result="ERFOLGREICH"
	elif [ -z "$t" ]; then
		result="BUILD FEHLGESCHLAGEN"
	else
		result="FEHLGESCHLAGEN"
	fi
	[ $status -ne 0 ] && FAILED_TASKS+=("$task")

	TOTAL=$((TOTAL + ${t:-0}))
	PASSED=$((PASSED + ${p:-0}))
	FAILED=$((FAILED + ${f:-0}))

	echo ""
	echo "$LINE"
	echo " AUFGABE:      $task"
	echo " BESCHREIBUNG: $(description "$task")"
	echo " STATUS:       $result"
	echo " TESTS:        ${t:-0} insgesamt, ${p:-0} erfolgreich, ${f:-0} fehlgeschlagen"
	echo "$LINE"
	echo ""
done

if [ ${#FAILED_TASKS[@]} -eq 0 ]; then
	echo "✅ Alle Tests erfolgreich ($PASSED/$TOTAL)."
else
	echo "❌ Fehlgeschlagen: ${FAILED_TASKS[*]} ($FAILED von $TOTAL Tests fehlgeschlagen)"
	exit 1
fi
