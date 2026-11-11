import { DurationPipe } from './duration-pipe';

describe('DurationPipe', () => {
  const pipe = new DurationPipe();

  it('formats minutes, hours and both', () => {
    expect(pipe.transform(45)).toBe('45m');
    expect(pipe.transform(120)).toBe('2h');
    expect(pipe.transform(95)).toBe('1h 35m');
  });
});
