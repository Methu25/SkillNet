import { useState } from 'react';
import { profileImageApi } from '../../../api/profileImageApi';
import DeleteProfileImageDialog from './DeleteProfileImageDialog';
import ProfileAvatar from './ProfileAvatar';
import ProfileImageDialog from './ProfileImageDialog';
import './ProfileImageManager.css';

const ProfileImageManager = ({ imagePath, candidateName, onChanged, onNotify }) => {
    const [dialog, setDialog] = useState(null);
    const [working, setWorking] = useState(false);
    const [error, setError] = useState('');

    const close = () => { if (!working) { setDialog(null); setError(''); } };

    const upload = async (file) => {
        setWorking(true);
        setError('');
        try {
            await profileImageApi.upload(file);
            setDialog(null);
            await onChanged();
            onNotify('Profile picture updated successfully.');
        } catch (requestError) {
            setError(requestError.message || 'The profile picture could not be uploaded. Please reselect and try again.');
        } finally {
            setWorking(false);
        }
    };

    const remove = async () => {
        setWorking(true);
        setError('');
        try {
            await profileImageApi.remove();
            setDialog(null);
            await onChanged();
            onNotify('Profile picture deleted. Your default avatar is now displayed.');
        } catch (requestError) {
            setError(requestError.message || 'The profile picture could not be deleted.');
        } finally {
            setWorking(false);
        }
    };

    return (
        <>
            <ProfileAvatar imagePath={imagePath} name={candidateName} large />
            <div className="profile-image-actions">
                <button className="candidate-button candidate-button--secondary" onClick={() => { setError(''); setDialog('upload'); }}>{imagePath ? 'Change Profile Picture' : 'Upload Profile Picture'}</button>
                {imagePath && <button className="candidate-button profile-image-delete" onClick={() => { setError(''); setDialog('delete'); }}>Delete Profile Picture</button>}
            </div>
            {dialog === 'upload' && <ProfileImageDialog replacing={Boolean(imagePath)} uploading={working} requestError={error} onClose={close} onUpload={upload} />}
            {dialog === 'delete' && <DeleteProfileImageDialog deleting={working} error={error} onClose={close} onConfirm={remove} />}
        </>
    );
};

export default ProfileImageManager;
