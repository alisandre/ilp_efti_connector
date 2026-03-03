import logo from '../../assets/logo.png'; // Sostituisci con il percorso corretto dell'immagine

interface Props {
  className?: string;
}

/**
 * Logo ILP Consulting — sostituito con immagine statica.
 */
export function IlpLogo({ className }: Props) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center'}} className={className}>
      <img
        src={logo}
        alt="ILP Consulting Logo"
        style={{ width: 'auto', height: '50px' }}
      />
    </div>
  );
}
