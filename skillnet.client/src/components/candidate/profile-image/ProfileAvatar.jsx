import { useState } from 'react';
import { resolveApiUrl } from '../../../api/apiClient';

const ProfileAvatar = ({ imagePath, name = 'Candidate', large = false, className = '' }) => {
    const [failedImageUrl, setFailedImageUrl] = useState('');
    const imageUrl = resolveApiUrl(imagePath);
    const initials = name.split(' ').filter(Boolean).slice(0, 2).map(part => part[0]).join('').toUpperCase() || 'CN';

    return (
        <div className={`candidate-avatar${large ? ' candidate-avatar--large' : ''} ${className}`.trim()}>
            {imageUrl && failedImageUrl !== imageUrl
                ? <img src={imageUrl} alt={`${name} profile`} onError={() => setFailedImageUrl(imageUrl)} />
                : <span>{initials}</span>}
        </div>
    );
};

export default ProfileAvatar;
