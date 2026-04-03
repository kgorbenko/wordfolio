const CENTER = 200;
const OUTER_RADIUS = 192;
const RING_RADIUS = 172;
const INNER_RADIUS = 76;
const BLADE_COUNT = 8;

const generateBlades = (): string[] =>
    Array.from({ length: BLADE_COUNT }, (_, i) => {
        const step = (2 * Math.PI) / BLADE_COUNT;
        const start = i * step - Math.PI / 2;
        const end = start + step * 0.78;
        const mid = (start + end) / 2;
        const controlR = (RING_RADIUS + INNER_RADIUS) * 0.53;

        const x1 = CENTER + RING_RADIUS * Math.cos(start);
        const y1 = CENTER + RING_RADIUS * Math.sin(start);
        const x2 = CENTER + INNER_RADIUS * Math.cos(end);
        const y2 = CENTER + INNER_RADIUS * Math.sin(end);
        const cx = CENTER + controlR * Math.cos(mid);
        const cy = CENTER + controlR * Math.sin(mid);

        return `M ${x1.toFixed(1)} ${y1.toFixed(1)} Q ${cx.toFixed(1)} ${cy.toFixed(1)} ${x2.toFixed(1)} ${y2.toFixed(1)}`;
    });

type Tick = {
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    major: boolean;
};

const generateTicks = (): Tick[] => {
    const count = 72;
    return Array.from({ length: count }, (_, i) => {
        const angle = ((i * 5 - 90) * Math.PI) / 180;
        const major = i % 9 === 0;
        const outer = OUTER_RADIUS;
        const inner = major ? OUTER_RADIUS - 8 : OUTER_RADIUS - 4;

        return {
            x1: CENTER + outer * Math.cos(angle),
            y1: CENTER + outer * Math.sin(angle),
            x2: CENTER + inner * Math.cos(angle),
            y2: CENTER + inner * Math.sin(angle),
            major,
        };
    });
};

const blades = generateBlades();
const ticks = generateTicks();

type ApertureRingProps = {
    readonly className?: string;
};

export const ApertureRing = ({ className }: ApertureRingProps) => (
    <svg
        className={className}
        viewBox="0 0 400 400"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
    >
        <defs>
            <radialGradient id="aperture-core-glow" cx="50%" cy="50%" r="50%">
                <stop offset="0%" stopColor="#B5F507" stopOpacity="0.06" />
                <stop offset="40%" stopColor="#B5F507" stopOpacity="0.02" />
                <stop offset="100%" stopColor="#B5F507" stopOpacity="0" />
            </radialGradient>
        </defs>

        <circle
            cx={CENTER}
            cy={CENTER}
            r={INNER_RADIUS + 10}
            fill="url(#aperture-core-glow)"
        />

        <circle
            cx={CENTER}
            cy={CENTER}
            r={OUTER_RADIUS}
            stroke="#B5F507"
            strokeWidth="0.3"
            opacity="0.1"
        />

        {ticks.map((tick, i) => (
            <line
                key={`t${i}`}
                x1={tick.x1.toFixed(1)}
                y1={tick.y1.toFixed(1)}
                x2={tick.x2.toFixed(1)}
                y2={tick.y2.toFixed(1)}
                stroke="#B5F507"
                strokeWidth={tick.major ? "0.7" : "0.35"}
                opacity={tick.major ? "0.24" : "0.1"}
                strokeLinecap="round"
            />
        ))}

        <circle
            cx={CENTER}
            cy={CENTER}
            r={RING_RADIUS}
            stroke="#B5F507"
            strokeWidth="1.2"
            opacity="0.55"
        />

        <circle
            cx={CENTER}
            cy={CENTER}
            r={RING_RADIUS + 3}
            stroke="#B5F507"
            strokeWidth="0.3"
            opacity="0.15"
        />

        {blades.map((d, i) => (
            <path
                key={`b${i}`}
                d={d}
                stroke="#B5F507"
                strokeWidth="0.7"
                opacity="0.3"
                fill="none"
                strokeLinecap="round"
            />
        ))}

        <circle
            cx={CENTER}
            cy={CENTER}
            r={INNER_RADIUS}
            stroke="#B5F507"
            strokeWidth="0.5"
            opacity="0.18"
        />

        <circle
            cx={CENTER}
            cy={CENTER}
            r={INNER_RADIUS - 4}
            stroke="#B5F507"
            strokeWidth="0.3"
            opacity="0.08"
            strokeDasharray="2 6"
        />

        <circle cx={CENTER} cy={CENTER} r="2.5" fill="#B5F507" opacity="0.12" />

        <circle
            cx={CENTER}
            cy={CENTER}
            r={RING_RADIUS}
            stroke="#E91E8C"
            strokeWidth="1.6"
            opacity="0.1"
            strokeDasharray="28 330"
            strokeDashoffset="60"
            strokeLinecap="round"
        />

        <circle
            cx={CENTER}
            cy={CENTER}
            r={RING_RADIUS}
            stroke="#E91E8C"
            strokeWidth="1"
            opacity="0.06"
            strokeDasharray="16 342"
            strokeDashoffset="200"
            strokeLinecap="round"
        />

        <text
            x={CENTER + OUTER_RADIUS + 4}
            y={CENTER - 2}
            fill="#B5F507"
            opacity="0.12"
            fontSize="5"
            fontFamily="monospace"
            letterSpacing="0.08em"
        >
            ƒ
        </text>
        <text
            x={CENTER - 5}
            y={CENTER - OUTER_RADIUS - 6}
            fill="#B5F507"
            opacity="0.1"
            fontSize="4.5"
            fontFamily="monospace"
            letterSpacing="0.1em"
            textAnchor="middle"
        >
            LEXICON
        </text>
    </svg>
);
