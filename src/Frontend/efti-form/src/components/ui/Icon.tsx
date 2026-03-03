interface Props {
  name: string
  /** Optional extra CSS classes */
  className?: string
  /** filled variant */
  filled?: boolean
  /** Size in px — any number e.g. 16, 18, 20, 24 */
  size?: number
}

/**
 * Google Material Symbols Rounded icon.
 * Usage: <Icon name="send" />  <Icon name="person" filled />
 */
export function Icon({ name, className = '', filled = false, size = 24 }: Props) {
  return (
    <span
      className={`material-symbols-rounded select-none${className ? ` ${className}` : ''}`}
      style={{
        fontSize: size,
        fontVariationSettings: `'FILL' ${filled ? 1 : 0}, 'wght' 300, 'GRAD' 0, 'opsz' ${size}`,
      }}
      aria-hidden
    >
      {name}
    </span>
  )
}
