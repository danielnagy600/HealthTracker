/**
 * ISO időbélyeg → helyi idő 'HH:mm' formában.
 * Az Angular `{{ érték | date: 'HH:mm' }}` pipe megfelelője.
 */
export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23'
  });
}
