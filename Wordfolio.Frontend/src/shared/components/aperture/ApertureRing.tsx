const center = 200;
const outerRadius = 192;
const ringRadius = 172;
const innerRadius = 76;
const bladeCount = 8;

const generateBlades = (): string[] =>
    Array.from({ length: bladeCount }, (_, i) => {
        const step = (2 * Math.PI) / bladeCount;
        const start = i * step - Math.PI / 2;
        const end = start + step * 0.78;
        const mid = (start + end) / 2;
        const controlRadius = (ringRadius + innerRadius) * 0.53;

        const x1 = center + ringRadius * Math.cos(start);
        const y1 = center + ringRadius * Math.sin(start);
        const x2 = center + innerRadius * Math.cos(end);
        const y2 = center + innerRadius * Math.sin(end);
        const cx = center + controlRadius * Math.cos(mid);
        const cy = center + controlRadius * Math.sin(mid);

        return `M ${x1.toFixed(1)} ${y1.toFixed(1)} Q ${cx.toFixed(1)} ${cy.toFixed(1)} ${x2.toFixed(1)} ${y2.toFixed(1)}`;
    });

type Tick = {
    readonly x1: number;
    readonly y1: number;
    readonly x2: number;
    readonly y2: number;
    readonly major: boolean;
};

const generateTicks = (): Tick[] => {
    const count = 72;

    return Array.from({ length: count }, (_, i) => {
        const angle = ((i * 5 - 90) * Math.PI) / 180;
        const major = i % 9 === 0;
        const outer = outerRadius;
        const inner = major ? outerRadius - 8 : outerRadius - 4;

        return {
            x1: center + outer * Math.cos(angle),
            y1: center + outer * Math.sin(angle),
            x2: center + inner * Math.cos(angle),
            y2: center + inner * Math.sin(angle),
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
                <stop offset="0%" stopColor="#16DB93" stopOpacity="0.06" />
                <stop offset="40%" stopColor="#16DB93" stopOpacity="0.02" />
                <stop offset="100%" stopColor="#16DB93" stopOpacity="0" />
            </radialGradient>
        </defs>

        <circle
            cx={center}
            cy={center}
            r={innerRadius + 10}
            fill="url(#aperture-core-glow)"
        />

        <circle
            cx={center}
            cy={center}
            r={outerRadius}
            stroke="#16DB93"
            strokeWidth="0.3"
            opacity="0.1"
        />

        {ticks.map((tick, i) => (
            <line
                key={`tick-${i}`}
                x1={tick.x1.toFixed(1)}
                y1={tick.y1.toFixed(1)}
                x2={tick.x2.toFixed(1)}
                y2={tick.y2.toFixed(1)}
                stroke="#16DB93"
                strokeWidth={tick.major ? "0.7" : "0.35"}
                opacity={tick.major ? "0.24" : "0.1"}
                strokeLinecap="round"
            />
        ))}

        <circle
            cx={center}
            cy={center}
            r={ringRadius}
            stroke="#16DB93"
            strokeWidth="1.2"
            opacity="0.55"
        />

        <circle
            cx={center}
            cy={center}
            r={ringRadius + 3}
            stroke="#16DB93"
            strokeWidth="0.3"
            opacity="0.15"
        />

        {blades.map((path, i) => (
            <path
                key={`blade-${i}`}
                d={path}
                stroke="#16DB93"
                strokeWidth="0.7"
                opacity="0.3"
                fill="none"
                strokeLinecap="round"
            />
        ))}

        <circle
            cx={center}
            cy={center}
            r={innerRadius}
            stroke="#16DB93"
            strokeWidth="0.5"
            opacity="0.18"
        />

        <circle
            cx={center}
            cy={center}
            r={innerRadius - 4}
            stroke="#16DB93"
            strokeWidth="0.25"
            opacity="0.06"
        />

        <circle cx={center} cy={center} r="2.5" fill="#16DB93" opacity="0.12" />

        <circle
            cx={center}
            cy={center}
            r={ringRadius}
            stroke="#E91E8C"
            strokeWidth="1.6"
            opacity="0.1"
            strokeDasharray="28 330"
            strokeDashoffset="60"
            strokeLinecap="round"
        />

        <circle
            cx={center}
            cy={center}
            r={ringRadius}
            stroke="#E91E8C"
            strokeWidth="1"
            opacity="0.06"
            strokeDasharray="16 342"
            strokeDashoffset="200"
            strokeLinecap="round"
        />

        <text
            x={center + outerRadius + 4}
            y={center - 2}
            fill="#16DB93"
            opacity="0.1"
            fontSize="5"
            fontFamily="monospace"
            letterSpacing="0.08em"
        >
            ƒ
        </text>
    </svg>
);
